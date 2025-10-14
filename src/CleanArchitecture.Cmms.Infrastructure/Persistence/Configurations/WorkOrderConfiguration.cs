using CleanArchitecture.Cmms.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.Configurations
{
    internal sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrders");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(w => w.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(w => w.TechnicianId)
                   .IsRequired(false);

            builder.Property(w => w.AssetId)
                  .IsRequired(true);

            // Value Object: Location
            builder.OwnsOne(w => w.Location, location =>
            {
                location.Property(l => l.Building)
                        .HasColumnName("Building");

                location.Property(l => l.Floor)
                        .HasColumnName("Floor");

                location.Property(l => l.Room)
                        .HasColumnName("Room");
            });

            // Child Entity: TaskStep
            builder.OwnsMany(w => w.Steps, step =>
            {
                step.ToTable("WorkOrderSteps");

                step.WithOwner().HasForeignKey("WorkOrderId");
                step.HasKey("Id");

                step.Property(s => s.Id)
                    .ValueGeneratedNever();

                step.Property(s => s.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                step.Property(s => s.Completed)
                    .IsRequired();

                //step.Property<DateTime>("CreatedOn"); //TODO:

                step.HasIndex("WorkOrderId");
            });

            // Child Entity: Comment
            builder.OwnsMany(w => w.Comments, comment =>
            {
                comment.ToTable("WorkOrderComments");

                comment.WithOwner().HasForeignKey("WorkOrderId");
                comment.HasKey("Id");

                comment.Property(c => c.Id)
                       .ValueGeneratedNever();

                comment.Property(c => c.Text)
                       .IsRequired()
                       .HasMaxLength(500);

                comment.Property(c => c.AuthorId)
                       .IsRequired();

                comment.Property<DateTime>("CreatedOn");

                comment.HasIndex("WorkOrderId");
            });

            builder.Navigation(w => w.Steps).Metadata.SetField("_steps");
            builder.Navigation(w => w.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(w => w.Comments).Metadata.SetField("_comments");
            builder.Navigation(w => w.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}