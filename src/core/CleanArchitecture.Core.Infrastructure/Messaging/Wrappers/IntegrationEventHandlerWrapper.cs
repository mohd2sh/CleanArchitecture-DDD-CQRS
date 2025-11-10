using CleanArchitecture.Core.Application.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Core.Infrastructure.Messaging.Wrappers;

/// <summary>
/// Concrete wrapper that knows TEvent at compile time.
/// </summary>
internal sealed class IntegrationEventHandlerWrapper<TEvent> : IntegrationEventHandlerWrapperBase
{
    public override async Task Handle(
        object @event,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedEvent = (TEvent)@event;

        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(typedEvent, cancellationToken);
        }
    }
}

