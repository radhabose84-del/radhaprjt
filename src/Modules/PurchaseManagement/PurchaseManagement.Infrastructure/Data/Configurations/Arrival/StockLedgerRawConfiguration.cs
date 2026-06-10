using PurchaseManagement.Domain.Entities.Arrival;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Arrival
{
    /// <summary>
    /// StockLedgerRaw does NOT extend BaseEntity — no audit / soft-delete columns (spec design decision).
    /// </summary>
    public class StockLedgerRawConfiguration : IEntityTypeConfiguration<StockLedgerRaw>
    {
        public void Configure(EntityTypeBuilder<StockLedgerRaw> b)
        {
            b.ToTable("StockLedgerRaw", "Purchase");
            b.HasKey(x => x.Id);

            b.Property(x => x.UnitId).IsRequired();
            b.Property(x => x.DocDate).IsRequired();
            b.Property(x => x.LotNo).IsRequired();
            b.Property(x => x.BaleNo);
            b.Property(x => x.BarcodeNumber);
            b.Property(x => x.BaleWeight).HasPrecision(18, 3);
            b.Property(x => x.BaleCaptureMethodId);   // nullable — Individual bales only

            // ── Cross-module FK columns (no DB constraint) ──
            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.UomId).IsRequired();

            b.Property(x => x.DocType)
                .IsRequired()
                .HasColumnType("varchar(3)")
                .HasDefaultValue("ARV");

            // ── Indexes ──
            b.HasIndex(x => x.LotNo);
            b.HasIndex(x => x.ItemId);
            b.HasIndex(x => x.BarcodeNumber);
        }
    }
}
