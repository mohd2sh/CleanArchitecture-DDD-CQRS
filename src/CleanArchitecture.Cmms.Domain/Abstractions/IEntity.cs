
namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    public interface IEntity<TId>
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        TId Id { get; }

        void ClearDomainEvents();
    }
}