using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class PreventiveSchedulerDtlConfiguration : IEntityTypeConfiguration<PreventiveSchedulerDetail>
    {
        public void Configure(EntityTypeBuilder<PreventiveSchedulerDetail> builder)
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

            builder.ToTable("PreventiveSchedulerDetail", "Maintenance");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.PreventiveSchedulerHeaderId)
                .HasColumnName("PreventiveSchedulerHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MachineId)
                .HasColumnName("MachineId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkOrderCreationStartDate)
                .HasColumnName("WorkOrderCreationStartDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ActualWorkOrderDate)
                .HasColumnName("ActualWorkOrderDate")
                .HasColumnType("date");

            builder.Property(t => t.MaterialReqStartDays)
                .HasColumnName("MaterialReqStartDays")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.RescheduleReason)
                .HasColumnName("RescheduleReason")
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.LastMaintenanceActivityDate)
                .HasColumnName("LastMaintenanceActivityDate")
                .HasColumnType("date");

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

            builder.Property(b => b.ScheduleId)
           .HasColumnName("ScheduleId")
           .HasColumnType("int")
           .IsRequired(false);
            builder.Property(b => b.FrequencyTypeId)
                .HasColumnName("FrequencyTypeId")
                .HasColumnType("int")
                .IsRequired(false);
            builder.Property(b => b.FrequencyInterval)
                .HasColumnName("FrequencyInterval")
                .HasColumnType("int")
                .IsRequired();
            builder.Property(b => b.FrequencyUnitId)
                .HasColumnName("FrequencyUnitId")
                .HasColumnType("int")
                .IsRequired(false);

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


            builder.HasOne(t => t.PreventiveScheduler)
            .WithMany(t => t.PreventiveSchedulerDetails)
            .HasForeignKey(t => t.PreventiveSchedulerHeaderId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Machine)
           .WithMany(t => t.PreventiveSchedulerDetail)
           .HasForeignKey(t => t.MachineId)
           .OnDelete(DeleteBehavior.Restrict);
                
                 builder.HasOne(b => b.MiscSchedule)
                .WithMany(b => b.PreventiveDetailSchedule)
                .HasForeignKey(b => b.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscFrequencyType)
                .WithMany(b => b.PreventiveDetailFrequencyType)
                .HasForeignKey(b => b.FrequencyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.MiscFrequencyUnit)
                .WithMany(b => b.PreventiveDetailFrequencyUnit)
                .HasForeignKey(b => b.FrequencyUnitId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}