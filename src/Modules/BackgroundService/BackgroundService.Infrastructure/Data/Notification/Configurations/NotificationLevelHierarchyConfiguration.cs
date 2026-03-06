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
    public class NotificationLevelHierarchyConfiguration : IEntityTypeConfiguration<NotificationLevelHierarchy>
    {
        public void Configure(EntityTypeBuilder<NotificationLevelHierarchy> builder)
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

            builder.ToTable("NotificationLevelHierarchy", "AppNotification");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NotificationConfigId)
            .HasColumnName("NotificationConfigId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.TargetTypeId)
            .HasColumnName("TargetTypeId")
            .HasColumnType("int")
            .IsRequired();


            builder.Property(t => t.TargetId)
            .HasColumnName("TargetId")
            .HasColumnType("int")
            .IsRequired();
          
            builder.Property(t => t.ApprovalModeId)
            .HasColumnName("ApprovalModeId")
            .HasColumnType("int")
            .IsRequired();


            builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasColumnType("Varchar(Max)")
            .IsRequired(false);

            builder.HasMany(h => h.NotificationEventRules)
            .WithOne(e => e.NotificationLevelHierarchy)
            .HasForeignKey(e => e.NotificationLevelHierarchyId)
            .OnDelete(DeleteBehavior.Cascade);

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

            builder.HasOne(ac => ac.NotificationConfig)
              .WithMany(am => am.NotificationLevelHierarchies)
              .HasForeignKey(ac => ac.NotificationConfigId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ac => ac.TargetType)
            .WithMany(am => am.TargetType)
            .HasForeignKey(ac => ac.TargetTypeId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ac => ac.ApprovalMode)
           .WithMany(am => am.ApprovalMode)
           .HasForeignKey(ac => ac.ApprovalModeId)
           .OnDelete(DeleteBehavior.NoAction);
        }
    }
}