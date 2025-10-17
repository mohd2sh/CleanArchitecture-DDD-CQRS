namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    internal abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    {
        public DateTime CreatedOn { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? LastModifiedOn { get; private set; }
        public string? LastModifiedBy { get; private set; }

        protected AuditableEntity() : base() { }

        protected AuditableEntity(TId id) : base(id) { }

        public void SetCreated(DateTime createdOn, string? createdBy)
        {
            CreatedOn = createdOn;
            CreatedBy = createdBy;
        }

        public void SetLastModified(DateTime modifiedOn, string? modifiedBy)
        {
            LastModifiedOn = modifiedOn;
            LastModifiedBy = modifiedBy;
        }
    }
}
