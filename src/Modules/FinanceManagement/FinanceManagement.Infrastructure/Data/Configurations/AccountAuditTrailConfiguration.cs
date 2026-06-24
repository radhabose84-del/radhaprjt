using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class AccountAuditTrailConfiguration : IEntityTypeConfiguration<AccountAuditTrail>
    {
        public void Configure(EntityTypeBuilder<AccountAuditTrail> b)
        {
            // The table carries an INSTEAD-OF UPDATE/DELETE immutability trigger (added in the migration).
            // Declaring it tells EF Core 8 to skip the OUTPUT clause on insert (SQL Server forbids OUTPUT
            // on a table that has any trigger).
            b.ToTable("AccountAuditTrail", "Finance",
                tb => tb.HasTrigger("trg_AccountAuditTrail_Immutable"));
            b.HasKey(x => x.Id);

            b.Property(x => x.CompanyId).IsRequired();
            b.Property(x => x.EntityName).HasMaxLength(60).IsRequired();
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.Action).HasMaxLength(40).IsRequired();
            b.Property(x => x.PropertyName).HasMaxLength(200).IsRequired();
            // OldValue / NewValue intentionally left as nvarchar(max).
            b.Property(x => x.CreatedByName).HasMaxLength(100);
            b.Property(x => x.CreatedByRole).HasMaxLength(100);
            b.Property(x => x.CreatedIP).HasMaxLength(50);
            b.Property(x => x.CreatedDate).IsRequired();
            b.Property(x => x.Scope).HasMaxLength(40);
            b.Property(x => x.ScopeKey).HasMaxLength(120);

            // Per-account chronological history (AC-3).
            b.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedDate })
             .HasDatabaseName("IX_AccountAuditTrail_Entity_CreatedDate");

            // Date-range export by company (AC-4).
            b.HasIndex(x => new { x.CompanyId, x.CreatedDate })
             .HasDatabaseName("IX_AccountAuditTrail_Company_CreatedDate");
        }
    }
}
