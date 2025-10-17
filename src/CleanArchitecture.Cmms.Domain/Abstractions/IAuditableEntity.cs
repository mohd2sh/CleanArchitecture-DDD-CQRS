namespace CleanArchitecture.Cmms.Domain.Abstractions
{
    public interface IAuditableEntity
    {
        string? CreatedBy { get; }
        DateTime CreatedOn { get; }
        string? LastModifiedBy { get; }
        DateTime? LastModifiedOn { get; }

        void SetCreated(DateTime createdOn, string? createdBy);
        void SetLastModified(DateTime modifiedOn, string? modifiedBy);
    }
}