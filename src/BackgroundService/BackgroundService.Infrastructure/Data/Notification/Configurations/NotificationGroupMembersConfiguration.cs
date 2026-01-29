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
    public class NotificationGroupMembersConfiguration : IEntityTypeConfiguration<NotificationGroupMembers>
    {
        public void Configure(EntityTypeBuilder<NotificationGroupMembers> builder)
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

            builder.ToTable("NotificationGroupMembers", "AppNotification");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GroupId)
            .HasColumnName("GroupId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.UserId)
            .HasColumnName("UserId")
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

            builder.HasOne(ac => ac.Group)
            .WithMany(am => am.NotificationGroupMembers)
            .HasForeignKey(ac => ac.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
          
        }
    }
}