namespace CleanArchitecture.Cmms.Infrastructure.Messaging.Wrappers;

internal abstract class IntegrationEventHandlerWrapperBase
{
    public abstract Task Handle(
        object @event,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

