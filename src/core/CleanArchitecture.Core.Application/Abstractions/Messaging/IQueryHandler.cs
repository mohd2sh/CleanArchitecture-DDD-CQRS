namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken = default);
}
