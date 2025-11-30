using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
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

        var pipelines = serviceProvider
            .GetServices<IIntegrationEventPipeline<TEvent>>()
            .ToList();

        foreach (var handler in handlers)
        {
            PipelineDelegate pipeline = () => handler.Handle(typedEvent, cancellationToken);

            foreach (var pipelineBehavior in pipelines)
            {
                var currentPipeline = pipeline;
                pipeline = () => pipelineBehavior.Handle(typedEvent, currentPipeline, cancellationToken);
            }

            await pipeline();
        }
    }
}

