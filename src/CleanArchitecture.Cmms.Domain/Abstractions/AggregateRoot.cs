namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
    {
        protected AggregateRoot()
        {
        }
        protected AggregateRoot(TId id) : base(id)
        {
        }
    }
}
