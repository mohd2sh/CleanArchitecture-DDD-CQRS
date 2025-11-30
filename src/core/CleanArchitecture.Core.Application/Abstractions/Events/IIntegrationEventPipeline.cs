using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace CleanArchitecture.Core.Application.Abstractions.Events;

public interface IIntegrationEventPipeline<in TEvent>
{
    /// <summary>
    /// Handles the integration event through the pipeline.
    /// </summary>
    Task Handle(
        TEvent @event,
        PipelineDelegate next,
        CancellationToken cancellationToken = default);
}

