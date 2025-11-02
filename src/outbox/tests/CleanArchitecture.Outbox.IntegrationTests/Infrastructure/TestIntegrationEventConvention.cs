using CleanArchitecture.Core.Application.Abstractions.Events;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

internal sealed class TestIntegrationEventConvention : IIntegrationEventConvention
{
    public bool IsIntegrationEvent(Type type)
    {
        return true;
    }
}

