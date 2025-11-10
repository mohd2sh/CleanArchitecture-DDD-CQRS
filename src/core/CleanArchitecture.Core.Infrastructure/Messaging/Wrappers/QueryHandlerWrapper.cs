using CleanArchitecture.Core.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Core.Infrastructure.Messaging.Wrappers;

/// <summary>
/// Concrete wrapper that knows BOTH TQuery and TResult at compile time.
/// </summary>
internal sealed class QueryHandlerWrapper<TQuery, TResult> : QueryHandlerWrapperBase<TResult>
    where TQuery : IQuery<TResult>
{
    public override async Task<TResult> Handle(
        IQuery<TResult> query,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // Safe cast - the wrapper type is constructed with the exact query type
        var typedQuery = (TQuery)query;

        // Type-safe resolution
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        // Get generic pipelines (run for both commands and queries)
        var genericPipelines = serviceProvider
            .GetServices<IPipeline<TQuery, TResult>>()
            .ToList();

        // Get query-specific pipelines
        var queryPipelines = serviceProvider
            .GetServices<IQueryPipeline<TQuery, TResult>>()
            .ToList();

        // Combine: generic pipelines first, then query-specific pipelines
        // Reverse so outermost pipeline wraps innermost
        var allPipelines = ((IEnumerable<object>)genericPipelines)
            .Concat(queryPipelines)
            .Reverse()
            .ToList();

        // Build the pipeline chain (innermost = handler)
        PipelineDelegate<TResult> pipeline = () => handler.Handle(typedQuery, cancellationToken);

        foreach (var pipelineBehavior in allPipelines)
        {
            var currentPipeline = pipeline;

            // Check if it's a generic pipeline or query-specific pipeline
            if (pipelineBehavior is IPipeline<TQuery, TResult> genericPipeline)
            {
                pipeline = () => genericPipeline.Handle(typedQuery, currentPipeline, cancellationToken);
            }
            else if (pipelineBehavior is IQueryPipeline<TQuery, TResult> qryPipeline)
            {
                pipeline = () => qryPipeline.Handle(typedQuery, currentPipeline, cancellationToken);
            }
        }

        // Execute the pipeline
        return await pipeline();
    }
}

