
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster
{
    public class WorkOrderActivityConfiguration   : IEntityTypeConfiguration<WorkOrderActivity>
    {       
       public void Configure(EntityTypeBuilder<WorkOrderActivity> builder)
        {            
            
            builder.ToTable("WorkOrderActivity", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkOrderId)
                .HasColumnName("WorkOrderId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.WOActivity)
                .WithMany(am => am.WorkOrderActivities)
                .HasForeignKey(amg => amg.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.ActivityId)
                .HasColumnName("ActivityId")
                .HasColumnType("int")
                .IsRequired();  
            builder.HasOne(amg => amg.ActivityMaster)
                .WithMany(am => am.workOrderActivities)
                .HasForeignKey(amg => amg.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(1000)")
                .IsRequired(false);
        }
    }
}