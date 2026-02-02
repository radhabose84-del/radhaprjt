using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
public class PurchaseBillEntryDetailConfiguration 
    : IEntityTypeConfiguration<PurchaseBillEntryDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseBillEntryDetail> b)
    {
        b.ToTable("PurchaseBillEntryDetail", "Purchase");

        b.HasKey(x => x.Id);

        b.Property(x => x.PoQty).HasPrecision(18, 3);
        b.Property(x => x.GrnQty).HasPrecision(18, 3);
        b.Property(x => x.BilledQty).HasPrecision(18, 3);

        b.Property(x => x.PoRate).HasPrecision(18, 4);
        b.Property(x => x.BilledRate).HasPrecision(18, 4);

        b.Property(x => x.TaxPercentage).HasPrecision(5, 2);

        b.Property(x => x.LineBaseAmount).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxableAmount).HasPrecision(18, 2);
        b.Property(x => x.CgstAmount).HasPrecision(18, 2);
        b.Property(x => x.SgstAmount).HasPrecision(18, 2);
        b.Property(x => x.IgstAmount).HasPrecision(18, 2);
        b.Property(x => x.LineTotal).HasPrecision(18, 2);
    }
}
