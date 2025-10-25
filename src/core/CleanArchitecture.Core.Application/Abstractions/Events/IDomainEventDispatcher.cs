using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Core.Application.Abstractions.Events;

public interface IDomainEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
