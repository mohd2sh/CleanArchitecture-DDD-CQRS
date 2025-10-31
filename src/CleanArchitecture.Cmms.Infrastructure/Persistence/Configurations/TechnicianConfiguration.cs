using CleanArchitecture.Cmms.Domain.Technicians;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.Configurations;

internal sealed class TechnicianConfiguration : AuditableEntityConfiguration<Technician, Guid>
{
    protected override void ConfigureCore(EntityTypeBuilder<Technician> builder)
    {
        var schema = "technicians";

        builder.ToTable("Technicians", schema);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(t => t.Status)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(t => t.MaxConcurrentAssignments)
               .IsRequired();

        builder.OwnsOne(t => t.SkillLevel, skill =>
        {
            skill.Property(s => s.LevelName)
                 .HasColumnName("SkillLevelName")
                 .HasMaxLength(50)
                 .IsRequired();

            skill.Property(s => s.Rank)
                 .HasColumnName("SkillRank")
                 .IsRequired();
        });

        builder.OwnsMany(t => t.Certifications, cert =>
        {
            cert.ToTable("TechnicianCertifications", schema);

            cert.WithOwner().HasForeignKey("TechnicianId");

            cert.HasKey("TechnicianId", "Code");

            cert.Property(c => c.Code)
                .HasColumnName("CertificationCode")
                .HasMaxLength(50)
                .IsRequired();

            cert.Property(c => c.IssuedOn)
                .HasColumnName("IssuedOn")
                .IsRequired();

            cert.Property(c => c.ExpiresOn)
                .HasColumnName("ExpiresOn");

            cert.HasIndex("TechnicianId");
        });

        builder.OwnsMany(t => t.Assignments, assignment =>
        {
            assignment.ToTable("TechnicianAssignments", schema);

            assignment.WithOwner().HasForeignKey("TechnicianId");
            assignment.HasKey("Id");

            assignment.Property(a => a.Id)
                      .ValueGeneratedNever();

            assignment.Property(a => a.WorkOrderId)
                      .IsRequired();

            assignment.Property(a => a.AssignedOn)
                      .IsRequired();

            assignment.Property(a => a.CompletedOn)
                      .IsRequired(false);

            assignment.HasIndex("TechnicianId");
        });

        builder.Navigation(t => t.Assignments)
               .Metadata.SetField("_assignments");

        builder.Navigation(t => t.Assignments)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(t => t.Certifications)
               .Metadata.SetField("_certifications");

        builder.Navigation(t => t.Certifications)
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
