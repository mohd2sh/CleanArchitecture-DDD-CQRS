using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.DomainEvents;

public interface IDomainEventDispatcher
{
    Task Dispatch(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default);
}

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _publisher;
    public DomainEventDispatcher(IMediator publisher) => _publisher = publisher;
    public async Task Dispatch(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default)
    {
        //TODO:
        //foreach (var @event in events)
        //{
        //    await _publisher.Publish(events, ct);
        //}
    }
}
