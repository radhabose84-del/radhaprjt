using PurchaseManagement.Domain.Entities.Arrival;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Arrival
{
    /// <summary>
    /// ArrivalDetail does NOT extend BaseEntity — no audit / soft-delete columns (spec design decision).
    /// </summary>
    public class ArrivalDetailConfiguration : IEntityTypeConfiguration<ArrivalDetail>
    {
        public void Configure(EntityTypeBuilder<ArrivalDetail> b)
        {
            b.ToTable("ArrivalDetail", "Purchase");
            b.HasKey(x => x.Id);

            b.Property(x => x.ArrivalHeaderId).IsRequired();

            // ── One-to-many: header → details (Restrict) ──
            b.HasOne(x => x.ArrivalHeader)
                .WithMany(t => t.ArrivalDetails)
                .HasForeignKey(x => x.ArrivalHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.HsnId).IsRequired();
            b.Property(x => x.PackTypeId).IsRequired();
            b.Property(x => x.MixCodeId).IsRequired();
            b.Property(x => x.UomId).IsRequired();

            // ── Quantities / money ──
            b.Property(x => x.Rate).HasPrecision(18, 2);
            b.Property(x => x.OrderedQty).HasPrecision(18, 3);
            b.Property(x => x.ArrivedQty).HasPrecision(18, 3);
            b.Property(x => x.CancelledQty).HasPrecision(18, 3);
            b.Property(x => x.BalanceQty).HasPrecision(18, 3);

            // ── Consolidated bale range ──
            b.Property(x => x.BatchNumber)
                .HasColumnType("varchar(30)");   // nullable — Batch Number is optional
            b.Property(x => x.BaleNumberFrom);
            b.Property(x => x.BaleNumberTo);
            b.Property(x => x.TotalBaleCount);

            // ── Indexes ──
            b.HasIndex(x => x.ArrivalHeaderId);
            b.HasIndex(x => x.ItemId);
        }
    }
}
