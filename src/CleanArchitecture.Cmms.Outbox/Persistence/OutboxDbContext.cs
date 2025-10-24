using CleanArchitecture.Cmms.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Outbox.Persistence;

public sealed class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
            
            // Index for efficient querying of unprocessed messages
            entity.HasIndex(e => new { e.ProcessedAt, e.CreatedAt })
                .HasFilter("[ProcessedAt] IS NULL");
        });
    }
}
