using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

internal sealed class TestIntegrationEventDispatcher : IIntegrationEventDispatcher
{

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
