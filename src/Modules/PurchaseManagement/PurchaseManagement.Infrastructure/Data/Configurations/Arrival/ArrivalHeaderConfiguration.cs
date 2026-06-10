using PurchaseManagement.Domain.Entities.Arrival;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Arrival
{
    public class ArrivalHeaderConfiguration : IEntityTypeConfiguration<ArrivalHeader>
    {
        public void Configure(EntityTypeBuilder<ArrivalHeader> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("ArrivalHeader", "Purchase");
            b.HasKey(x => x.Id);

            // ── Scalar properties ──
            b.Property(x => x.UnitId).IsRequired();

            b.Property(x => x.ArrivalNumber)
                .IsRequired()
                .HasColumnType("varchar(30)");

            b.Property(x => x.ArrivalDate).IsRequired();

            b.Property(x => x.VehicleNumber)
                .IsRequired()
                .HasColumnType("varchar(30)");

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.SupplierId).IsRequired();
            b.Property(x => x.StationId).IsRequired();
            b.Property(x => x.GodownId).IsRequired();
            b.Property(x => x.TransporterId).IsRequired();

            b.Property(x => x.FreightRate).HasPrecision(18, 2);
            b.Property(x => x.InvoiceGstNo).HasColumnType("varchar(20)");
            b.Property(x => x.LrNumber).HasColumnType("varchar(30)");
            b.Property(x => x.ContainerNo).HasColumnType("varchar(30)");

            b.Property(x => x.LorryIn);
            b.Property(x => x.LorryOut);

            // ── Weighbridge ──
            b.Property(x => x.GrossWeight).HasPrecision(18, 3);
            b.Property(x => x.TareWeight).HasPrecision(18, 3);
            b.Property(x => x.NetWeight).HasPrecision(18, 3);
            b.Property(x => x.PartyWeight).HasPrecision(18, 3);
            b.Property(x => x.WeightDifference).HasPrecision(18, 3);
            b.Property(x => x.MoisturePercentage).HasPrecision(5, 2);

            b.Property(x => x.Remarks).HasColumnType("varchar(500)");

            // ── QC sign-off (header-level; mirrors GrnDetail) ──
            b.Property(x => x.QcAcceptedQuantity).HasPrecision(18, 3);
            b.Property(x => x.QcRejectedQuantity).HasPrecision(18, 3);
            b.Property(x => x.QcRejectedRemarks).HasColumnType("varchar(500)");
            b.Property(x => x.QcPersonName).HasColumnType("varchar(100)");
            b.Property(x => x.QcRemarks).HasColumnType("varchar(500)");
            b.Property(x => x.QcDate);
            b.Property(x => x.QcApprovedIp).HasColumnType("varchar(50)");
            b.Property(x => x.IsQcApproved).HasColumnType("bit").IsRequired();

            // Transient payload-only collection — never persisted.
            b.Ignore(x => x.StockRows);

            // ── Same-module FK constraints (Restrict) ──
            b.HasOne(x => x.RawMaterialPO)
                .WithMany()
                .HasForeignKey(x => x.RawMaterialPOId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.QcStatus)
                .WithMany()
                .HasForeignKey(x => x.QcStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ──
            b.HasIndex(x => x.ArrivalNumber).IsUnique();
            b.HasIndex(x => x.RawMaterialPOId);
            b.HasIndex(x => x.QcStatusId);
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
