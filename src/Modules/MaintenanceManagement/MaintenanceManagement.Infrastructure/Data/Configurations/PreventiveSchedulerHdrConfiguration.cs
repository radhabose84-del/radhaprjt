using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class PreventiveSchedulerHdrConfiguration : IEntityTypeConfiguration<PreventiveSchedulerHeader>
    {
        public void Configure(EntityTypeBuilder<PreventiveSchedulerHeader> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>
               (
                    v => v == Status.Active,  
                    v => v ? Status.Active : Status.Inactive 
                );

                var isDeletedConverter = new ValueConverter<IsDelete, bool>
                (
                 v => v == IsDelete.Deleted,  
                 v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
                );
                
            builder.ToTable("PreventiveSchedulerHeader", "Maintenance");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.PreventiveSchedulerName)
                .HasColumnName("PreventiveSchedulerName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(b => b.MachineGroupId)
                .HasColumnName("MachineGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.MaintenanceCategoryId)
                .HasColumnName("MaintenanceCategoryId")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.ScheduleId)
                .HasColumnName("ScheduleId")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.FrequencyTypeId)
                .HasColumnName("FrequencyTypeId")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.FrequencyInterval)
                .HasColumnName("FrequencyInterval")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.FrequencyUnitId)
                .HasColumnName("FrequencyUnitId")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.EffectiveDate)
                .HasColumnName("EffectiveDate")
                .HasColumnType("date")
                .IsRequired();
            builder.Property(b => b.GraceDays)
                .HasColumnName("GraceDays")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.ReminderWorkOrderDays)
                .HasColumnName("ReminderWorkOrderDays")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.ReminderMaterialReqDays)
                .HasColumnName("ReminderMaterialReqDays")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.IsDownTimeRequired)
                .HasColumnName("IsDownTimeRequired")
                .HasColumnType("bit")
                  .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

                  builder.Property(b => b.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(b => b.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

                 builder.Property(cf => cf.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(cf => cf.IsDeleted)
                 .HasColumnName("IsDeleted")
                 .HasColumnType("bit")
                 .HasConversion(isDeletedConverter)
                 .IsRequired();

            builder.Property(cf => cf.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(cf => cf.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(cf => cf.ModifiedByName)
                 .HasColumnType("varchar(50)");

            builder.Property(cf => cf.ModifiedIP)
                .HasColumnType("varchar(255)");
                
                builder.HasOne(b => b.MachineGroup)
                .WithMany(b => b.PreventiveSchedulerHeaders)
                .HasForeignKey(b => b.MachineGroupId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscMaintenanceCategory)
                .WithMany(b => b.MaintenanceCategory)
                .HasForeignKey(b => b.MaintenanceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscSchedule)
                .WithMany(b => b.Schedule)
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscFrequencyType)
                .WithMany(b => b.FrequencyType)
                .HasForeignKey(b => b.FrequencyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscFrequencyUnit)
                .WithMany(b => b.FrequencyUnit)
                .HasForeignKey(b => b.FrequencyUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}