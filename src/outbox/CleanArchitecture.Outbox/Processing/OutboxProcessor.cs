using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.Processing;

/// <summary>
/// Background service that processes outbox messages and publishes to integration event handlers.
/// Currently publishes to in-process handlers. Future: Can publish to message bus for external services.
/// Please refer to the ADRs and readme files for more info.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

        var messages = await outboxStore.GetUnprocessedAsync(10, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                _logger.LogDebug("Processing outbox message {MessageId} of type {EventType}",
                    message.Id, message.EventType);

                // Deserialize event
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError("Cannot find event type {EventType}", message.EventType);
                    await outboxStore.IncrementRetryAsync(message.Id, $"Event type not found: {message.EventType}", cancellationToken);
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions);
                if (domainEvent == null)
                {
                    _logger.LogError("Cannot deserialize event {EventType}", message.EventType);
                    await outboxStore.IncrementRetryAsync(message.Id, "Deserialization failed", cancellationToken);
                    continue;
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

                // Mark as processed
                await outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);

                _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                await outboxStore.IncrementRetryAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
