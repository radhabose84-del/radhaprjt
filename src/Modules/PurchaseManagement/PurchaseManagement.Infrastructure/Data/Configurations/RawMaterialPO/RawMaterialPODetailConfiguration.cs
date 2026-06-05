using PurchaseManagement.Domain.Entities.RawMaterialPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.RawMaterialPO
{
    public class RawMaterialPODetailConfiguration : IEntityTypeConfiguration<RawMaterialPODetail>
    {
        public void Configure(EntityTypeBuilder<RawMaterialPODetail> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("RawMaterialPODetail", "Purchase");
            b.HasKey(x => x.Id);

            b.Property(x => x.POHeaderId).IsRequired();

            // ── One-to-many: header → details (Restrict) ──
            b.HasOne(x => x.RawMaterialPOMaster)
                .WithMany(t => t.RawMaterialPODetails)
                .HasForeignKey(x => x.POHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.HsnId).IsRequired();

            // ── Quantities / money ──
            b.Property(x => x.Quantity).HasPrecision(18, 3);
            b.Property(x => x.Weight).HasPrecision(18, 3);
            b.Property(x => x.Rate).HasPrecision(18, 2);
            b.Property(x => x.ItemValue).HasPrecision(18, 2);
            b.Property(x => x.CGSTPercentage).HasPrecision(5, 2);
            b.Property(x => x.SGSTPercentage).HasPrecision(5, 2);
            b.Property(x => x.IGSTPercentage).HasPrecision(5, 2);
            b.Property(x => x.CGSTValue).HasPrecision(18, 2);
            b.Property(x => x.SGSTValue).HasPrecision(18, 2);
            b.Property(x => x.IGSTValue).HasPrecision(18, 2);
            b.Property(x => x.TotalGST).HasPrecision(18, 2);
            b.Property(x => x.NetValue).HasPrecision(18, 2);

            // ── Indexes ──
            b.HasIndex(x => x.POHeaderId);
            b.HasIndex(x => x.ItemId);
            b.HasIndex(x => x.HsnId);

            // ── Audit / soft-delete ──
            b.Property(x => x.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            b.Property(x => x.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            b.Property(x => x.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            b.Property(x => x.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

            b.Property(x => x.ModifiedByName)
                .HasColumnType("varchar(50)");

            b.Property(x => x.ModifiedIP)
                .HasColumnType("varchar(20)");
        }
    }
}
