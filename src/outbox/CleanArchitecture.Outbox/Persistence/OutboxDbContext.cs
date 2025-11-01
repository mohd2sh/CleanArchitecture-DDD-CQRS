using CleanArchitecture.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.Persistence;

public sealed class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Payload)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.ProcessedAt);

            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0);

            entity.Property(e => e.LastError)
                .HasMaxLength(2000);

            entity.Property(e => e.MaxRetries)
                .HasDefaultValue(3);

            entity.HasIndex(e => new { e.ProcessedAt, e.RetryCount, e.CreatedAt })
                .HasFilter("[ProcessedAt] IS NULL");
        });

        modelBuilder.Entity<DeadLetterMessage>(entity =>
        {
            entity.ToTable("DeadLetterMessages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Payload)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.MovedToDeadLetterAt)
                .IsRequired();

            entity.Property(e => e.RetryCount)
                .HasDefaultValue(0);

            entity.Property(e => e.LastError)
                .HasMaxLength(2000);

            entity.Property(e => e.MaxRetries)
                .HasDefaultValue(3);

            entity.HasIndex(e => e.MovedToDeadLetterAt);
        });
    }
}
