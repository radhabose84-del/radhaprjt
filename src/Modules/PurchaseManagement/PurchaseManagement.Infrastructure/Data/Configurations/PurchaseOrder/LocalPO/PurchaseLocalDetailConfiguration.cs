using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchaseLocalDetailConfiguration : IEntityTypeConfiguration<PurchaseLocalDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseLocalDetail> b)
    {
        b.ToTable("PurchaseLocalDetail", "Purchase");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.PurchaseLocal)
        .WithMany(m => m.Details)
        .HasForeignKey(x => x.PurchaseLocalId)
        .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MiscDiscountType)
         .WithMany(m => m.PurchaseLocalDetailDiscount)
         .HasForeignKey(x => x.DiscountTypeId)
         .OnDelete(DeleteBehavior.NoAction);

        b.Property(x => x.Quantity).HasPrecision(18, 2);
        b.Property(x => x.UnitPrice).HasPrecision(18, 3);
        b.Property(x => x.LastPOPrice).HasPrecision(18, 3);

        b.Property(x => x.ItemValue).HasPrecision(18, 2);
        b.Property(x => x.DiscountValue).HasPrecision(18, 2);
        b.Property(x => x.PandFCharge).HasPrecision(18, 2);
        b.Property(x => x.OtherCharge).HasPrecision(18, 2);
        b.Property(x => x.CGST).HasPrecision(18, 2);
        b.Property(x => x.SGST).HasPrecision(18, 2);
        b.Property(x => x.IGST).HasPrecision(18, 2);
    }
}