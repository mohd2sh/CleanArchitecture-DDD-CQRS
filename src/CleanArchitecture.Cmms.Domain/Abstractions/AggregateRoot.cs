namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    {
        protected AggregateRoot()
        {
        }
        protected AggregateRoot(TId id) : base(id)
        {
        }
    }
}
