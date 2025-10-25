using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Core.Application.Abstractions.Events;

/// <summary>
/// Handler for integration events that execute asynchronously via outbox pattern.
/// 
/// CURRENT: Events written to outbox table, processed by background worker, published to same process.
/// FUTURE: Background worker publishes to message bus (RabbitMQ/Service Bus) for external services.
/// 
/// Handlers implementing this interface are:
/// - Not executed immediately (deferred via outbox)
/// - Guaranteed delivery (survives restarts)
/// - Retryable (automatic retry on failure)
/// - Suitable for: Emails, notifications, external system updates
/// </summary>
/// <typeparam name="TEvent">The domain event type</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the integration event asynchronously.
    /// Called by outbox processor after event is read from outbox table.
    /// </summary>
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
