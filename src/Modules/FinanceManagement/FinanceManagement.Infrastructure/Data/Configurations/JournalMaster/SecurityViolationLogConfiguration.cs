using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-10 — written by DB triggers. Minimal, FK-free so the trigger insert never fails.
    public class SecurityViolationLogConfiguration : IEntityTypeConfiguration<SecurityViolationLog>
    {
        public void Configure(EntityTypeBuilder<SecurityViolationLog> builder)
        {
            builder.ToTable("SecurityViolationLog", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.TableName).HasColumnName("TableName").HasColumnType("varchar(50)").IsRequired();
            builder.Property(t => t.JournalHeaderId).HasColumnName("JournalHeaderId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.AttemptedAction).HasColumnName("AttemptedAction").HasColumnType("varchar(10)").IsRequired();
            builder.Property(t => t.UserName).HasColumnName("UserName").HasColumnType("varchar(100)").IsRequired();
            builder.Property(t => t.AttemptedAt).HasColumnName("AttemptedAt").IsRequired();
            builder.Property(t => t.Channel).HasColumnName("Channel").HasColumnType("varchar(30)").IsRequired(false);

            builder.HasIndex(t => t.JournalHeaderId);
            builder.HasIndex(t => t.AttemptedAt);
        }
    }
}
