using BackgroundService.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data.Notification.Configurations
{
    // EF configuration
       public class NotificationEventLogConfiguration : IEntityTypeConfiguration<NotificationEventLog>
       {
       public void Configure(EntityTypeBuilder<NotificationEventLog> builder)
       {
              builder.ToTable("NotificationEventLog", "AppNotification");

              builder.HasKey(t => t.Id);

              builder.Property(t => t.NotificationLevelRuleId)
                     .HasColumnName("NotificationLevelRuleId")
                     .HasColumnType("int")
                     .IsRequired(false); // <- nullable

              builder.HasOne(e => e.NotificationEventRules)
                     .WithMany(r => r.NotificationEventLog)
                     .HasForeignKey(e => e.NotificationLevelRuleId)
                     .OnDelete(DeleteBehavior.SetNull); // <- allow parent delete, set null

              builder.Property(t => t.UnitId).IsRequired();
              builder.Property(t => t.ChannelId).IsRequired();
              builder.HasOne(e => e.Channel)
                     .WithMany(m => m.Channel)
                     .HasForeignKey(e => e.ChannelId)
                     .OnDelete(DeleteBehavior.NoAction);

              builder.Property(t => t.ActionStatus).HasColumnType("varchar(250)").IsRequired(false);
              builder.Property(t => t.NotificationStatusId).IsRequired();
              builder.HasOne(e => e.NotificationStatus)
                     .WithMany(m => m.NotificationStatus)
                     .HasForeignKey(e => e.NotificationStatusId)
                     .OnDelete(DeleteBehavior.NoAction);

              builder.Property(t => t.SendTo).HasColumnType("varchar(1000)").IsRequired(false);
              builder.Property(t => t.MessageText).HasColumnType("varchar(max)").IsRequired(false);

              builder.Property(t => t.ReadStatusId).IsRequired();
              builder.HasOne(e => e.ReadStatus)
                     .WithMany(m => m.ReadStatus)
                     .HasForeignKey(e => e.ReadStatusId)
                     .OnDelete(DeleteBehavior.NoAction);

              builder.Property(t => t.Timestamp).HasColumnType("datetimeoffset").IsRequired();

              // base fields left as you had (IsActive/IsDeleted/Created*)
       }
}
}
