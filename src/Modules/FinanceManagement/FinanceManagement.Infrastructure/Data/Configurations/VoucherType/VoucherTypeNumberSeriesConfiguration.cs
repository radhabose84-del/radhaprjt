using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.VoucherType
{
    public class VoucherTypeNumberSeriesConfiguration : IEntityTypeConfiguration<VoucherTypeNumberSeries>
    {
        public void Configure(EntityTypeBuilder<VoucherTypeNumberSeries> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VoucherTypeNumberSeries", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.VoucherTypeId)
                .HasColumnName("VoucherTypeId").HasColumnType("int").IsRequired();

            builder.Property(t => t.FinancialYearId)
                .HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();

            builder.Property(t => t.LastUsedNumber)
                .HasColumnName("LastUsedNumber").HasColumnType("int").HasDefaultValue(0).IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit")
                .HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // One counter per (voucher type, fiscal year) among non-deleted rows
            builder.HasIndex(t => new { t.VoucherTypeId, t.FinancialYearId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => t.VoucherTypeId);

            // Same-module FK -> VoucherTypeMaster (parent collection)
            builder.HasOne(t => t.VoucherType)
                .WithMany(v => v.NumberSeries)
                .HasForeignKey(t => t.VoucherTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FinancialYearId is a cross-module reference (UserManagement) — no DB FK constraint.
        }
    }
}
