using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Core.Infrastructure.Messaging;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetRequiredService<IEnumerable<IDomainEventHandler<TEvent>>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }
}
