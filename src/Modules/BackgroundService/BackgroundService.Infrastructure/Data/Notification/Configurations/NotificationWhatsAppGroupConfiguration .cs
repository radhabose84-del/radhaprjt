using BackgroundService.Core.Domain.Entities.Notifications;
using BackgroundService.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data.Notification.Configurations
{
    public class NotificationWhatsAppGroupConfiguration : IEntityTypeConfiguration<NotificationWhatsAppGroup>
    {
        public void Configure(EntityTypeBuilder<NotificationWhatsAppGroup> builder)
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

            builder.ToTable("NotificationWhatsAppGroup", "AppNotification");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GroupName)
                .HasColumnName("GroupName")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property<string>("ApiKey")
                .HasColumnType("varchar(max)")
                .HasColumnName("ApiKey");
                
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
        }
    }
}
