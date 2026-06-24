using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-16B — lean engine output (NOT a BaseEntity).
    public class JournalFlagConfiguration : IEntityTypeConfiguration<JournalFlag>
    {
        public void Configure(EntityTypeBuilder<JournalFlag> builder)
        {
            builder.ToTable("JournalFlag", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.JournalHeaderId).HasColumnName("JournalHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RuleTypeId).HasColumnName("RuleTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.Value).HasColumnName("Value").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.FlaggedAt).HasColumnName("FlaggedAt").IsRequired();
            builder.Property(t => t.DigestSent).HasColumnName("DigestSent").HasColumnType("bit").HasDefaultValue(false).IsRequired();

            builder.HasIndex(t => t.JournalHeaderId);
            builder.HasIndex(t => t.FlaggedAt);

            builder.HasOne(t => t.JournalHeader)
                .WithMany()
                .HasForeignKey(t => t.JournalHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RuleType)
                .WithMany()
                .HasForeignKey(t => t.RuleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
