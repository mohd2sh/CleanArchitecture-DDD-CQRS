using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Core.Application.Abstractions.Events;

/// <summary>
/// Dispatcher for integration events that execute asynchronously via outbox pattern.
/// </summary>
public interface IIntegrationEventDispatcher
{
    /// <summary>
    /// Publishes a strongly-typed domain event to integration event handlers.
    /// </summary>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a generic integration event (for system events, external events, etc.).
    /// </summary>
    Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default);
}

