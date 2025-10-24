using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Infrastructure.Messaging.Wrappers;

/// <summary>
/// Concrete wrapper that knows BOTH TCommand and TResult at compile time.
/// </summary>
internal sealed class CommandHandlerWrapper<TCommand, TResult> : CommandHandlerWrapperBase<TResult>
    where TCommand : ICommand<TResult>
{
    public override async Task<TResult> Handle(
        ICommand<TResult> command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // Safe cast - the wrapper type is constructed with the exact command type
        var typedCommand = (TCommand)command;

        // Type-safe resolution - we have both TCommand and TResult!
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        // Get generic pipelines (run for both commands and queries)
        var genericPipelines = serviceProvider
            .GetServices<IPipeline<TCommand, TResult>>()
            .ToList();

        // Get command-specific pipelines
        var commandPipelines = serviceProvider
            .GetServices<ICommandPipeline<TCommand, TResult>>()
            .ToList();

        // Combine: generic pipelines first, then command-specific pipelines
        // Reverse so outermost pipeline wraps innermost
        var allPipelines = ((IEnumerable<object>)genericPipelines)
            .Concat(commandPipelines)
            .Reverse()
            .ToList();

        // Build the pipeline chain (innermost = handler)
        PipelineDelegate<TResult> pipeline = () => handler.Handle(typedCommand, cancellationToken);

        foreach (var pipelineBehavior in allPipelines)
        {
            var currentPipeline = pipeline;

            // Check if it's a generic pipeline or command-specific pipeline
            if (pipelineBehavior is IPipeline<TCommand, TResult> genericPipeline)
            {
                pipeline = () => genericPipeline.Handle(typedCommand, currentPipeline, cancellationToken);
            }
            else if (pipelineBehavior is ICommandPipeline<TCommand, TResult> cmdPipeline)
            {
                pipeline = () => cmdPipeline.Handle(typedCommand, currentPipeline, cancellationToken);
            }
        }

        // Execute the pipeline
        return await pipeline();
    }
}

