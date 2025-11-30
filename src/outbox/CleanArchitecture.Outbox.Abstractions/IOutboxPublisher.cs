namespace CleanArchitecture.Outbox.Abstractions;

/// <summary>
/// Interface for publishing integration events from outbox to message bus or in-memory handlers.
/// Abstracts the message bus implementation (MassTransit, etc.)
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes an integration event to the message bus or in-memory handlers.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}


