using BackgroundService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Notification.Configurations
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessages", "AppNotification");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .UseIdentityColumn();

            builder.Property(x => x.ConsumerName)
                   .HasColumnType("varchar(200)")
                   .IsRequired();

            builder.Property(x => x.MessageId)
                   .IsRequired();

            builder.Property(x => x.CorrelationId)
                   .IsRequired(false);

            builder.Property(x => x.ProcessedAt)
                   .HasColumnType("datetimeoffset")
                   .IsRequired();

            // Unique constraint: one successful processing per (consumer, message)
            builder.HasIndex(x => new { x.ConsumerName, x.MessageId })
                   .IsUnique()
                   .HasDatabaseName("UQ_InboxMessages_Consumer_MessageId");
        }
    }
}
