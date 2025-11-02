using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

public sealed class TestDomainEvent : IDomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime? OccurredOn { get; set; } = DateTime.UtcNow;
    public string Data { get; set; } = string.Empty;
}


