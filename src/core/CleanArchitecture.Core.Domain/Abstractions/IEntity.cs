namespace CleanArchitecture.Core.Domain.Abstractions;

public interface IEntity<TId>
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    TId Id { get; }

    void ClearDomainEvents();
}
