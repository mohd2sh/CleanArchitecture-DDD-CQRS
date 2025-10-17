namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class Entity<TId> : IHasDomainEvents, IEntity<TId>
    {
        public TId Id { get; protected set; } = default!;

        protected Entity() { }

        protected Entity(TId id)
        {
            Id = id;
        }

        private readonly List<IDomainEvent> _events = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;
        protected void Raise(IDomainEvent e) => _events.Add(e);
        public void ClearDomainEvents() => _events.Clear();
    }
}