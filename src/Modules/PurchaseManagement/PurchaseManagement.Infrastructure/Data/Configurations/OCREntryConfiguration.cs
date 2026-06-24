using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Purchase
{
    public class OCREntryConfiguration : IEntityTypeConfiguration<OCREntry>
    {
        public void Configure(EntityTypeBuilder<OCREntry> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("OCREntry", "Purchase");
            b.HasKey(x => x.Id);

            // ── Scalar properties ──
            b.Property(x => x.OcrNumber)
                .IsRequired()
                .HasColumnType("varchar(20)");

            b.Property(x => x.OcrDate).IsRequired();

            b.Property(x => x.Quantity).HasPrecision(18, 3);
            b.Property(x => x.Weight).HasPrecision(18, 3);
            b.Property(x => x.Rate).HasPrecision(18, 2);

            b.Property(x => x.BrokerName)
                .HasColumnType("varchar(100)");

            b.Property(x => x.DocumentPath)
                .HasColumnType("varchar(500)");

            // ── Additional Cotton Details — scalar fields ──
            b.Property(x => x.MillSampleNo)
                .HasColumnType("varchar(50)");

            b.Property(x => x.CottonPassedBy)
                .HasColumnType("varchar(100)");

            b.Property(x => x.Remarks)
                .HasColumnType("varchar(500)");

            b.Property(x => x.GstPercentage).HasPrecision(5, 2);
            b.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
            b.Property(x => x.InsurancePercentage).HasPrecision(5, 2);

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.UomId);               // Inventory UOM (rate unit)
            b.Property(x => x.QualityTemplateId);   // QC.QualityTemplate

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.SupplierId).IsRequired();
            b.Property(x => x.LocationId).IsRequired();
            b.Property(x => x.StationId).IsRequired();
            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.CountId).IsRequired();
            b.Property(x => x.PackTypeId);   // Production.PackType — optional, no DB constraint

            // ── Same-module FK constraints (Restrict) ──
            b.HasOne(x => x.ProcurementSource)
                .WithMany()
                .HasForeignKey(x => x.ProcurementSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ProcurementType)
                .WithMany()
                .HasForeignKey(x => x.ProcurementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Grade)
                .WithMany()
                .HasForeignKey(x => x.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.OcrStatus)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.PaymentTerm)
                .WithMany()
                .HasForeignKey(x => x.PaymentTermId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Additional Cotton Details — same-module MiscMaster FKs (nullable, Restrict) ──
            b.HasOne(x => x.PaymentMode)
                .WithMany()
                .HasForeignKey(x => x.PaymentModeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Weighment)
                .WithMany()
                .HasForeignKey(x => x.WeighmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TransitInsurance)
                .WithMany()
                .HasForeignKey(x => x.TransitInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.LorryFreight)
                .WithMany()
                .HasForeignKey(x => x.LorryFreightId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ModeOfTransport)
                .WithMany()
                .HasForeignKey(x => x.ModeOfTransportId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ──
            b.HasIndex(x => x.OcrNumber).IsUnique();
            b.HasIndex(x => x.ProcurementSourceId);
            b.HasIndex(x => x.SupplierId);
            b.HasIndex(x => x.StatusId);
            b.HasIndex(x => x.OcrDate);

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
