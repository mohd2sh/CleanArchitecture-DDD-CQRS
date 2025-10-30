namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

/// <summary>
/// Pipeline behavior that executes only for queries.
/// Useful for query-specific cross-cutting concerns like caching, result transformation, etc.
/// </summary>
public interface IQueryPipeline<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(
        TQuery query,
        PipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default);
}
