using CleanArchitecture.Cmms.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.Configurations
{
    internal abstract class AuditableEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
        where TEntity : AggregateRoot<TId>
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {

            builder.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.Property(e => e.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(e => e.LastModifiedOn)
                .IsRequired(false);

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasIndex(e => e.CreatedOn);

            ConfigureCore(builder);
        }

        protected abstract void ConfigureCore(EntityTypeBuilder<TEntity> builder);
    }
}
