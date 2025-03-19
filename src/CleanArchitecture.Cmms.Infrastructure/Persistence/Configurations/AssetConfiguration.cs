using CleanArchitecture.Cmms.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.Configurations
{
    internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.ToTable("Assets");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(a => a.Type)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(a => a.Status)
                   .HasConversion<string>()
                   .IsRequired();

            // Value Object: AssetTag
            builder.OwnsOne(a => a.Tag, tag =>
            {
                tag.Property(t => t.Value)
                   .HasColumnName("Tag")
                   .HasMaxLength(100)
                   .IsRequired();
            });

            // Value Object: AssetLocation
            builder.OwnsOne(a => a.Location, location =>
            {
                location.Property(l => l.Site)
                        .HasColumnName("Site")
                        .HasMaxLength(100)
                        .IsRequired();

                location.Property(l => l.Area)
                        .HasColumnName("Area")
                        .HasMaxLength(100)
                        .IsRequired();

                location.Property(l => l.Zone)
                        .HasColumnName("Zone")
                        .HasMaxLength(100)
                        .IsRequired();
            });

            // Owned Collection: MaintenanceRecord
            builder.OwnsMany(a => a.MaintenanceRecords, record =>
            {
                record.ToTable("MaintenanceRecords");

                record.WithOwner().HasForeignKey("AssetId");
                record.HasKey("Id");

                record.Property(r => r.Id)
                      .ValueGeneratedNever();

                record.Property(r => r.AssetId)
                      .IsRequired();

                record.Property(r => r.StartedOn)
                      .IsRequired();

                record.Property(r => r.Description)
                      .HasMaxLength(500)
                      .IsRequired();

                record.Property(r => r.PerformedBy)
                      .HasMaxLength(150)
                      .IsRequired();

                record.HasIndex("AssetId");
            });

            builder.Navigation(a => a.MaintenanceRecords)
                   .Metadata.SetField("_maintenanceRecords");
            builder.Navigation(a => a.MaintenanceRecords)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
