using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data.Notification.Configurations
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
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

            builder.ToTable("NotificationTemplate", "AppNotification");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
    
            builder.Property(t => t.NotificationTypeId)
            .HasColumnName("NotificationTypeId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.NotificationConfigId)
            .HasColumnName("NotificationConfigId")
            .HasColumnType("int")
            .IsRequired();
            builder.HasOne(ac => ac.NotificationConfig)
            .WithMany(am => am.NotificationTemplates)
            .HasForeignKey(ac => ac.NotificationConfigId)
            .OnDelete(DeleteBehavior.Restrict); 

              builder.Property(t => t.SubjectTemplate)
            .HasColumnName("SubjectTemplate")
            .HasColumnType("NVarchar(Max)")
            .IsRequired();

            builder.Property(t => t.HeaderTemplate)
            .HasColumnName("HeaderTemplate")
            .HasColumnType("NVarchar(Max)")
            .IsRequired();

              builder.Property(t => t.BodyTemplate)
            .HasColumnName("BodyTemplate")
            .HasColumnType("NVarchar(Max)")
            .IsRequired();

              builder.Property(t => t.LanguageCode)
            .HasColumnName("LanguageCode")
            .HasColumnType("Varchar(50)")
            .IsRequired();

            builder.Property(t => t.FooterTemplate)
            .HasColumnName("FooterTemplate")
            .HasColumnType("NVarchar(Max)")
            .IsRequired();

            builder.Property(x => x.IsTable).HasDefaultValue(false);

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

            builder.HasOne(ac => ac.NotificationType)
                .WithMany(am => am.NotificationTemplates)
                .HasForeignKey(ac => ac.NotificationTypeId)
                .OnDelete(DeleteBehavior.Cascade);          
        }
    }
}