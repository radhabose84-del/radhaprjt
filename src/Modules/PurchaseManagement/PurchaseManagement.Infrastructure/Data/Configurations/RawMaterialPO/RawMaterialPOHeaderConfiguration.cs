using PurchaseManagement.Domain.Entities.RawMaterialPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.RawMaterialPO
{
    public class RawMaterialPOHeaderConfiguration : IEntityTypeConfiguration<RawMaterialPOHeader>
    {
        public void Configure(EntityTypeBuilder<RawMaterialPOHeader> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("RawMaterialPOHeader", "Purchase");
            b.HasKey(x => x.Id);

            // ── Scalar properties ──
            b.Property(x => x.UnitId).IsRequired();

            b.Property(x => x.PONumber)
                .IsRequired()
                .HasColumnType("varchar(20)");

            b.Property(x => x.PODate).IsRequired();

            b.Property(x => x.TaxableTotal).HasPrecision(18, 2);
            b.Property(x => x.TotalGstAmount).HasPrecision(18, 2);
            b.Property(x => x.NetTotal).HasPrecision(18, 2);

            b.Property(x => x.Remarks)
                .HasColumnType("varchar(500)");

            // ── Same-module FK constraints (Restrict) ──
            b.HasOne(x => x.Ocr)
                .WithMany()
                .HasForeignKey(x => x.OcrId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ProcurementDocumentType)
                .WithMany()
                .HasForeignKey(x => x.ProcurementDocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ConversionStatus)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ──
            b.HasIndex(x => x.PONumber).IsUnique();
            b.HasIndex(x => x.OcrId);
            b.HasIndex(x => x.ProcurementDocumentTypeId);
            b.HasIndex(x => x.StatusId);
            b.HasIndex(x => x.UnitId);

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
