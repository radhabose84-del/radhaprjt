using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BlanketPO;

public class PurchaseBlanketDetailConfiguration : IEntityTypeConfiguration<PurchaseBlanketDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseBlanketDetail> b)
    {
        b.ToTable("PurchaseBlanketDetail", "Purchase");
        b.HasKey(x => x.Id);

        // Quantity/Price precision
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 3);
        b.Property(x => x.ItemValue).HasPrecision(18, 2);
        b.Property(x => x.DiscountValue).HasPrecision(18, 2);
        b.Property(x => x.PandFCharge).HasPrecision(18, 2);
        b.Property(x => x.OtherCharge).HasPrecision(18, 2);
        b.Property(x => x.CGST).HasPrecision(18, 2);
        b.Property(x => x.SGST).HasPrecision(18, 2);
        b.Property(x => x.IGST).HasPrecision(18, 2);
        b.Property(x => x.GSTPercentage).HasPrecision(5, 2);
        b.Property(x => x.CGSTPercentage).HasPrecision(5, 2);
        b.Property(x => x.SGSTPercentage).HasPrecision(5, 2);
        b.Property(x => x.IGSTPercentage).HasPrecision(5, 2);

        // FK — PurchaseBlanketHeaderId → PurchaseBlanketHeader (parent-child, cascade)
        b.HasOne(x => x.PurchaseBlanketHeader)
            .WithMany(m => m.Details)
            .HasForeignKey(x => x.PurchaseBlanketHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK — BlanketDetailId → BlanketDetail (links to blanket line, no cascade)
        b.HasOne(x => x.BlanketDetail)
            .WithMany()
            .HasForeignKey(x => x.BlanketDetailId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — DiscountTypeId → MiscMaster
        b.HasOne(x => x.MiscDiscountType)
            .WithMany(m => m.PurchaseBlanketDetailDiscount)
            .HasForeignKey(x => x.DiscountTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        b.HasIndex(x => x.PurchaseBlanketHeaderId);
        b.HasIndex(x => x.BlanketDetailId);
    }
}
