using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Outbox.Abstractions;
using CleanArchitecture.Outbox.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.Processing;

/// <summary>
/// Core processor for outbox messages. Contains the processing logic separated from BackgroundService.
/// </summary>
public sealed class OutboxMessageProcessor : IOutboxProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public OutboxMessageProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxMessageProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 10;
        int processedCount = 0;

        // Process one record at a time up to batchSize
        // Each message is locked during processing, preventing duplicate processing
        while (processedCount < batchSize)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            // Start transaction for this single message with READ COMMITTED isolation level
            // Required for READPAST lock hint to work (allows concurrent workers to skip locked rows)
            using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var outboxStore = scope.ServiceProvider.GetRequiredService<ICustomOutboxStore>();

            Guid? messageId = null;
            try
            {
                var message = await outboxStore.GetNextUnprocessedMessageAsync(cancellationToken);

                if (message == null)
                {
                    break;
                }

                messageId = message.Id;

                await ProcessMessage(message, scope, cancellationToken);

                await outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox");

                if (IsSqlException(ex) || !messageId.HasValue)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }

                await outboxStore.IncrementRetryAsync(messageId.Value, ex.Message, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                processedCount++;
            }
        }
    }

    private async Task ProcessMessage(OutboxMessage message, IServiceScope scope, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing outbox message {MessageId} of type {EventType}",
            message.Id, message.EventType);

        var eventType = Type.GetType(message.EventType);
        if (eventType == null)
        {
            throw new InvalidOperationException($"Event type not found: {message.EventType}");
        }

        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions);
        if (domainEvent == null)
        {
            throw new InvalidOperationException("Deserialization failed");
        }

        // Find and invoke integration event handlers
        var handlerInterfaceType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = scope.ServiceProvider.GetServices(handlerInterfaceType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerInterfaceType.GetMethod("Handle");
            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, new[] { domainEvent, cancellationToken })!;
            }
        }
    }

    private static bool IsSqlException(Exception ex)
    {
        return ex is SqlException || ex is DbUpdateException;
    }
}

