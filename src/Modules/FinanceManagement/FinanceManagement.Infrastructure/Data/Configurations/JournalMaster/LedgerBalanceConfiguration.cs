using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-09 — running ledger aggregate. NOT a BaseEntity: no soft delete / audit columns.
    public class LedgerBalanceConfiguration : IEntityTypeConfiguration<LedgerBalance>
    {
        public void Configure(EntityTypeBuilder<LedgerBalance> builder)
        {
            builder.ToTable("LedgerBalance", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.GlAccountId).HasColumnName("GlAccountId").HasColumnType("int").IsRequired();
            builder.Property(t => t.AccountingPeriodId).HasColumnName("AccountingPeriodId").HasColumnType("int").IsRequired();
            builder.Property(t => t.CostCentreId).HasColumnName("CostCentreId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.FinancialYearId).HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();
            builder.Property(t => t.DrTotal).HasColumnName("DrTotal").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.CrTotal).HasColumnName("CrTotal").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.Balance).HasColumnName("Balance").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();

            builder.Property(t => t.RowVersion).HasColumnName("RowVersion").IsRowVersion();

            // One balance bucket per account/period/cost-centre. SQL Server treats NULLs as equal in a
            // unique index, so a single "no cost centre" (NULL) row is allowed per (company, account, period).
            builder.HasIndex(t => new { t.CompanyId, t.GlAccountId, t.AccountingPeriodId, t.CostCentreId })
                .IsUnique()
                .HasDatabaseName("UX_LedgerBalance");

            builder.HasIndex(t => t.FinancialYearId);

            builder.HasOne(t => t.GlAccount)
                .WithMany()
                .HasForeignKey(t => t.GlAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AccountingPeriod)
                .WithMany()
                .HasForeignKey(t => t.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CostCentre)
                .WithMany()
                .HasForeignKey(t => t.CostCentreId)
                .OnDelete(DeleteBehavior.Restrict);

            // CompanyId / FinancialYearId are cross-module — no DB FK constraint.
        }
    }
}
