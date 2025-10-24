using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IMediator
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
