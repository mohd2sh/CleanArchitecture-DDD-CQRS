namespace CleanArchitecture.Core.Application.Abstractions.Events;

public interface IIntegrationEventHandler<in TEvent>
{
    /// <summary>
    /// Handles the integration event asynchronously.
    /// </summary>
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
