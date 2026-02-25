using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class PreventiveScheduleLogConfiguration : IEntityTypeConfiguration<PreventiveScheduleLog>
    {
        public void Configure(EntityTypeBuilder<PreventiveScheduleLog> builder)
        {
             builder.ToTable("PreventiveScheduleLog", "Maintenance");
            builder.HasKey(t => t.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired(true);

                 builder.Property(t => t.PreventiveScheduleId)
                .HasColumnName("PreventiveScheduleId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.PreventiveScheduleDetailId)
                .HasColumnName("PreventiveScheduleDetailId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ActionType)
                .HasColumnName("ActionType")
                .HasColumnType("varchar(50)")
                .IsRequired(true);

            builder.Property(t => t.ChangedFields)
                .HasColumnName("ChangedFields")
                .HasColumnType("nvarchar(max)")
                .IsRequired(true);

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(max)")
                .IsRequired(false);

            builder.Property(t => t.Source)
                .HasColumnName("Source")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.IsSuccess)
                .HasColumnName("IsSuccess")
                .HasColumnType("bit");

            builder.Property(t => t.ErrorMessage)
                .HasColumnName("ErrorMessage")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

                 builder.Property(t => t.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasColumnType("int");

                 builder.Property(t => t.CreatedDate)
                .HasColumnName("CreatedDate")
                .HasColumnType("datetimeoffset");

                 builder.Property(t => t.CreatedByName)
                .HasColumnName("CreatedByName")
                .HasColumnType("varchar(50)");

                 builder.Property(t => t.CreatedIP)
                .HasColumnName("CreatedIP")
                .HasColumnType("varchar(255)");

        }
    }
}