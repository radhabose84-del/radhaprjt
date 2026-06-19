using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.VoucherType
{
    public class VoucherTypeMasterConfiguration : IEntityTypeConfiguration<VoucherTypeMaster>
    {
        public void Configure(EntityTypeBuilder<VoucherTypeMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VoucherTypeMaster", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.VoucherTypeCode)
                .HasColumnName("VoucherTypeCode").HasColumnType("varchar(10)").IsRequired();

            builder.Property(t => t.VoucherTypeName)
                .HasColumnName("VoucherTypeName").HasColumnType("varchar(100)").IsRequired();

            builder.Property(t => t.NumberPadding)
                .HasColumnName("NumberPadding").HasColumnType("int").IsRequired();

            builder.Property(t => t.IsSystem)
                .HasColumnName("IsSystem").HasColumnType("bit").HasDefaultValue(false).IsRequired();

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

            // VoucherTypeCode (also the series prefix) is unique per company among non-deleted rows
            builder.HasIndex(t => new { t.CompanyId, t.VoucherTypeCode })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => t.CompanyId);
        }
    }
}
