namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : MediatR.IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{ }
