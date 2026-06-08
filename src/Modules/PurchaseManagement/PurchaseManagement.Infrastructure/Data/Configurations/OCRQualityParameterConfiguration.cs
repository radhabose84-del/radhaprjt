using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Purchase
{
    public class OCRQualityParameterConfiguration : IEntityTypeConfiguration<OCRQualityParameter>
    {
        public void Configure(EntityTypeBuilder<OCRQualityParameter> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("OCRQualityParameter", "Purchase");
            b.HasKey(x => x.Id);

            // ── Scalar / cross-module FK columns ──
            b.Property(x => x.QualityTemplateId).IsRequired();   // cross-module — no constraint
            b.Property(x => x.ParamId).IsRequired();             // cross-module — no constraint

            b.Property(x => x.Value)
                .HasColumnType("varchar(200)");

            // ── Same-module FK to parent OCR — child rows cascade-delete with the OCR ──
            b.HasOne(x => x.Ocr)
                .WithMany(o => o.OcrQualityParameters)
                .HasForeignKey(x => x.OcrId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Indexes ──
            b.HasIndex(x => x.OcrId);
            b.HasIndex(x => x.QualityTemplateId);

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
