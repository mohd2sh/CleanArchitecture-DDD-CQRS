namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IQueryPipeline<TQuery, TResponse> : IPipeline<TQuery, TResponse> where TQuery : IQuery<TResponse>
{

}
