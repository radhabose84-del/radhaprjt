using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-11B — lean job log + idempotency ledger (NOT a BaseEntity).
    public class RecurringGenerationLogConfiguration : IEntityTypeConfiguration<RecurringGenerationLog>
    {
        public void Configure(EntityTypeBuilder<RecurringGenerationLog> builder)
        {
            builder.ToTable("RecurringGenerationLog", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.TemplateId).HasColumnName("TemplateId").HasColumnType("int").IsRequired();
            builder.Property(t => t.Period).HasColumnName("Period").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.GeneratedVoucherId).HasColumnName("GeneratedVoucherId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.GeneratedAt).HasColumnName("GeneratedAt").IsRequired();
            builder.Property(t => t.AutoPosted).HasColumnName("AutoPosted").HasColumnType("bit").HasDefaultValue(false).IsRequired();

            builder.HasIndex(t => new { t.CompanyId, t.TemplateId, t.Period })
                .IsUnique()
                .HasDatabaseName("UX_RecurringGenerationLog_CompanyTemplatePeriod");

            builder.HasIndex(t => t.GeneratedAt);

            builder.HasOne(t => t.Template)
                .WithMany()
                .HasForeignKey(t => t.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.GeneratedVoucher)
                .WithMany()
                .HasForeignKey(t => t.GeneratedVoucherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
