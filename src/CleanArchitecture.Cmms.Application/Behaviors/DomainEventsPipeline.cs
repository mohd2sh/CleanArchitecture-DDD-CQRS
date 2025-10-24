using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitecture.Cmms.Application.Abstractions.Events;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Outbox.Abstractions;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Behaviors;

public class DomainEventsPipeline<TCommand, TResult>
    : ICommandPipeline<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IOutboxStore _outboxStore;
    private readonly ILogger<DomainEventsPipeline<TCommand, TResult>> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public DomainEventsPipeline(
        IUnitOfWork uow,
        IDomainEventDispatcher eventDispatcher,
        IOutboxStore outboxStore,
        ILogger<DomainEventsPipeline<TCommand, TResult>> logger)
    {
        _uow = uow;
        _eventDispatcher = eventDispatcher;
        _outboxStore = outboxStore;
        _logger = logger;
    }

    public async Task<TResult> Handle(
        TCommand request,
        PipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var result = await next();

        // Accumulate integration events
        var integrationEvents = new List<IDomainEvent>();

        // Process events in batches until no new events are raised
        await ProcessDomainEventsInBatches(integrationEvents, cancellationToken);

        // Only write to outbox after ALL transactional processing succeeds
        await WriteIntegrationEventsToOutbox(integrationEvents, cancellationToken);

        return result;
    }

    private async Task ProcessDomainEventsInBatches(
        List<IDomainEvent> integrationEvents,
        CancellationToken cancellationToken)
    {
        int batchNumber = 1;

        while (true)
        {
            // Collect and clear events from all aggregates
            var domainEvents = _uow.CollectDomainEvents();

            if (domainEvents.Count == 0)
                break; // No more events to process

            _logger.LogDebug("Processing domain event batch {BatchNumber} with {EventCount} events",
                batchNumber, domainEvents.Count);

            foreach (var domainEvent in domainEvents)
            {
                // Publish to transactional handlers using dispatcher
                await _eventDispatcher.PublishAsync((dynamic)domainEvent, cancellationToken);

                // Always accumulate for integration handlers (deferred, via outbox)
                integrationEvents.Add(domainEvent);
            }

            batchNumber++;
        }
    }

    private async Task WriteIntegrationEventsToOutbox(
        List<IDomainEvent> integrationEvents,
        CancellationToken cancellationToken)
    {
        if (integrationEvents.Count == 0)
            return;

        _logger.LogDebug("Writing {Count} integration events to outbox", integrationEvents.Count);

        foreach (var domainEvent in integrationEvents)
        {
            var eventType = domainEvent.GetType();

            await _outboxStore.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = eventType.AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(domainEvent, eventType, SerializerOptions),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }

}
