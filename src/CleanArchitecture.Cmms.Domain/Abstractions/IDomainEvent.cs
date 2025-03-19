namespace CleanArchitecture.Cmms.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime? OccurredOn { get; }
}
