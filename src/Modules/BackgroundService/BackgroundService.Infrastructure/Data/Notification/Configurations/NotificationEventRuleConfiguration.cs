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
    public class NotificationEventRuleConfiguration : IEntityTypeConfiguration<NotificationEventRule>
    {
        public void Configure(EntityTypeBuilder<NotificationEventRule> builder)
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

            builder.ToTable("NotificationEventRule", "AppNotification");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NotificationLevelHierarchyId)
            .HasColumnName("NotificationLevelHierarchyId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.RecipientTypeId)
           .HasColumnName("RecipientTypeId")
           .HasColumnType("int")
           .IsRequired();
            builder.HasOne(ac => ac.RecipientType)
            .WithMany(am => am.RecipientType)
            .HasForeignKey(ac => ac.RecipientTypeId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Property(t => t.NotificationChannelId)
           .HasColumnName("NotificationChannelId")
           .HasColumnType("int")
           .IsRequired();
             builder.HasOne(ac => ac.Channel)
            .WithMany(am => am.Channels)
            .HasForeignKey(ac => ac.NotificationChannelId)
            .OnDelete(DeleteBehavior.NoAction);
          
            builder.Property(t => t.TemplateId)
           .HasColumnName("TemplateId")
           .HasColumnType("int")
           .IsRequired();
            builder.HasOne(ac => ac.NotificationTemplates)
            .WithMany(am => am.NotificationEventRules)
            .HasForeignKey(ac => ac.TemplateId)
            .OnDelete(DeleteBehavior.Restrict); 

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