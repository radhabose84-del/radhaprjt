using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GlAccountMasterConfiguration : IEntityTypeConfiguration<GlAccountMaster>
    {
        public void Configure(EntityTypeBuilder<GlAccountMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GlAccountMaster", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AccountTypeId)
                .HasColumnName("AccountTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AccountGroupId)
                .HasColumnName("AccountGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AccountCode)
                .HasColumnName("AccountCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.AccountName)
                .HasColumnName("AccountName")
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(500)");

            builder.Property(t => t.NormalBalanceId)
                .HasColumnName("NormalBalanceId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CurrencyTypeId)
                .HasColumnName("CurrencyTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SubLedgerTypeId)
                .HasColumnName("SubLedgerTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.IsCostCentreMandatory)
                .HasColumnName("IsCostCentreMandatory")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.IsTaxRelevant)
                .HasColumnName("IsTaxRelevant")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.IsInterCompany)
                .HasColumnName("IsInterCompany")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.IsReconciliationRequired)
                .HasColumnName("IsReconciliationRequired")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasIndex(t => new { t.CompanyId, t.AccountCode }).IsUnique();
            builder.HasIndex(t => new { t.CompanyId, t.AccountName }).IsUnique();
            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => t.AccountTypeId);
            builder.HasIndex(t => t.AccountGroupId);

            // Same-module FK relationships (with reverse navigation collections)
            builder.HasOne(t => t.AccountTypeMaster)
                .WithMany(atm => atm.GlAccountMasters)
                .HasForeignKey(t => t.AccountTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AccountGroup)
                .WithMany(ag => ag.GlAccountMasters)
                .HasForeignKey(t => t.AccountGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Two FKs to MiscMaster — each binds to its own reverse collection
            builder.HasOne(t => t.NormalBalanceMaster)
                .WithMany(mm => mm.GlAccountsAsNormalBalance)
                .HasForeignKey(t => t.NormalBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SubLedgerTypeMaster)
                .WithMany(mm => mm.GlAccountsAsSubLedgerType)
                .HasForeignKey(t => t.SubLedgerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Currency type dropdown -> CurrencyForexConfig master (US-GL02-12)
            builder.HasOne(t => t.CurrencyTypeConfig)
                .WithMany()
                .HasForeignKey(t => t.CurrencyTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
