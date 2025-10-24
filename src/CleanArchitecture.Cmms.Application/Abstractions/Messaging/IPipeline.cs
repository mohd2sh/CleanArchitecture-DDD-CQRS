namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IPipeline<in TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(
        TRequest request,
        PipelineDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
