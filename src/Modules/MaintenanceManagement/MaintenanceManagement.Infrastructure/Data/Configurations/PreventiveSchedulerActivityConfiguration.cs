using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class PreventiveSchedulerActivityConfiguration : IEntityTypeConfiguration<PreventiveSchedulerActivity>
    {
        public void Configure(EntityTypeBuilder<PreventiveSchedulerActivity> builder)
        {
            builder.ToTable("PreventiveSchedulerActivity", "Maintenance");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.PreventiveSchedulerHeaderId)
                .HasColumnName("PreventiveSchedulerHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ActivityId)
                .HasColumnName("ActivityId")
                .HasColumnType("int")
                .IsRequired();

                builder.HasOne(t => t.PreventiveScheduler)
                .WithMany(t => t.PreventiveSchedulerActivities)
                .HasForeignKey(t => t.PreventiveSchedulerHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(t => t.Activity)
                .WithMany(t => t.PreventiveSchedulerActivities)
                .HasForeignKey(t => t.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}