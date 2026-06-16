using FinanceManagement.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.Outbox
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", "Finance");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.CorrelationId).IsRequired();
            builder.Property(x => x.EventType).IsRequired().HasMaxLength(500);
            builder.Property(x => x.EventData).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasDefaultValue(OutboxMessageStatus.Pending);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.LastError).HasMaxLength(2000);
            builder.Property(x => x.ModuleName).HasMaxLength(100).HasDefaultValue("FinanceManagement");

            builder.HasIndex(x => new { x.Status, x.NextRetryAt })
                .HasDatabaseName("IX_Finance_OutboxMessages_Status_NextRetryAt")
                .HasFilter("[Status] = 0");

            builder.HasIndex(x => x.CorrelationId)
                .HasDatabaseName("IX_Finance_OutboxMessages_CorrelationId");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_Finance_OutboxMessages_CreatedAt");
        }
    }
}
