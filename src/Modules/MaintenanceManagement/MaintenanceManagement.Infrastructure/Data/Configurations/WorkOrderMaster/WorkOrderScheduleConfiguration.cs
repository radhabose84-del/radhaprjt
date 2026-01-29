using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster
{
    public class WorkOrderScheduleConfiguration : IEntityTypeConfiguration<WorkOrderSchedule>
    {       
       public void Configure(EntityTypeBuilder<WorkOrderSchedule> builder)
        {             
            builder.ToTable("WorkOrderSchedule", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkOrderId)
                .HasColumnName("WorkOrderId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.WOSchedule)
                .WithMany(am => am.WorkOrderSchedules)
                .HasForeignKey(amg => amg.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.StartTime)
                .HasColumnName("StartTime")                
                .HasColumnType("DateTimeOffset")
                .IsRequired();   
            
            builder.Property(t => t.EndTime)
                .HasColumnName("EndTime")                
                .IsRequired(false)
                .HasColumnType("DateTimeOffset");    

             builder.Property(c => c.IsCompleted)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);
                
            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.MiscStatus)
                .WithMany(am => am.WorkOrderScheduleStatus)
                .HasForeignKey(amg => amg.StatusId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}