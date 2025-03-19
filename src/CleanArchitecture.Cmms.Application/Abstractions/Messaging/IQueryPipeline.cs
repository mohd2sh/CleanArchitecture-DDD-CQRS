using MediatR;

namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IQueryPipeline<TQuery, TResponse>
    : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{ }
