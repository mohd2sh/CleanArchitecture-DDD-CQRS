using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot
    {
        [Timestamp]
        public byte[] RowVersion { get; protected set; } = default!;

        protected AggregateRoot()
        {
        }
        protected AggregateRoot(TId id) : base(id)
        {
        }
    }
}
