namespace CleanArchitecture.Core.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime? OccurredOn { get; }
}
