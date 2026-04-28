using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderTypeMasterConfiguration : IEntityTypeConfiguration<SalesOrderTypeMaster>
    {
        public void Configure(EntityTypeBuilder<SalesOrderTypeMaster> builder)
        {
            // Value converters for enums
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            // Table mapping
            builder.ToTable("SalesOrderTypeMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // ── Identification ────────────────────────────────────────────────
            builder.Property(t => t.SoTypeId)
                .HasColumnName("SoTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TaxTypeId)
                .HasColumnName("TaxTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TypeName)
                .HasColumnName("TypeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            // ── Type behavior ─────────────────────────────────────────────────
            builder.Property(t => t.AllowsDispatch)
                .HasColumnName("AllowsDispatch")
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(t => t.RequiresValidity)
                .HasColumnName("RequiresValidity")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.AllowZeroPrice)
                .HasColumnName("AllowZeroPrice")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.MinPrice)
                .HasColumnName("MinPrice")
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(t => t.MaxPrice)
                .HasColumnName("MaxPrice")
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(t => t.MaxQty)
                .HasColumnName("MaxQty")
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            builder.Property(t => t.AllowPriceOverride)
                .HasColumnName("AllowPriceOverride")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.OverrideLimitPercent)
                .HasColumnName("OverrideLimitPercent")
                .HasColumnType("decimal(5,2)")
                .IsRequired(false);

            builder.Property(t => t.ApprovalRequired)
                .HasColumnName("ApprovalRequired")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            // ── Mode behavior (Sales_Mode + Tax_Type derived from TaxTypeId — NOT stored) ──
            builder.Property(t => t.CurrencyRequired)
                .HasColumnName("CurrencyRequired")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.AllowIGST)
                .HasColumnName("AllowIGST")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.CountryMandatory)
                .HasColumnName("CountryMandatory")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.DefaultCurrencyId)
                .HasColumnName("DefaultCurrencyId")
                .HasColumnType("int")
                .IsRequired(false);

            // ── BaseEntity (IsActive / IsDeleted + audit) ─────────────────────
            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .HasDefaultValue(Status.Active)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .HasDefaultValue(IsDelete.NotDeleted)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // ── Indexes ───────────────────────────────────────────────────────
            // Composite filtered-unique on (SoTypeId, TaxTypeId) excluding soft-deleted rows
            builder.HasIndex(t => new { t.SoTypeId, t.TaxTypeId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_SalesOrderTypeMaster_SoType_TaxType");

            builder.HasIndex(t => t.TaxTypeId)
                .HasDatabaseName("IX_SalesOrderTypeMaster_TaxTypeId");

            builder.HasIndex(t => t.DefaultCurrencyId)
                .HasDatabaseName("IX_SalesOrderTypeMaster_DefaultCurrencyId");

            // ── Same-module FK — MiscMaster for SoType (DB constraint) ────────
            builder.HasOne(t => t.SoType)
                .WithMany(m => m.SalesOrderTypeMastersAsSoType)
                .HasForeignKey(t => t.SoTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Cross-module FKs (TaxTypeId, DefaultCurrencyId) — NO DB FK constraints ──
        }
    }
}
