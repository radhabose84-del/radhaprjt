using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
public class PurchaseBillEntryHeaderConfiguration 
    : IEntityTypeConfiguration<PurchaseBillEntryHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseBillEntryHeader> b)
    {
        b.ToTable("PurchaseBillEntryHeader", "Purchase");

        b.HasKey(x => x.Id);

        b.Property(x => x.BillNumber)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.BillDate)
            .HasColumnType("date");

        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountTotal).HasPrecision(18, 2);
        b.Property(x => x.TaxableAmount).HasPrecision(18, 2);
        b.Property(x => x.CgstAmount).HasPrecision(18, 2);
        b.Property(x => x.SgstAmount).HasPrecision(18, 2);
        b.Property(x => x.IgstAmount).HasPrecision(18, 2);
        b.Property(x => x.OtherCharges).HasPrecision(18, 2);
        b.Property(x => x.RoundOff).HasPrecision(18, 2);
        b.Property(x => x.GrandTotal).HasPrecision(18, 2);

        b.Property(x => x.AttachmentPath)
            .HasMaxLength(250);

        b.Property(x => x.Remarks)
            .HasMaxLength(500);

        b.Property(x => x.IsBillAccounted)
            .HasDefaultValue(false);

        // 1-many
        b.HasMany(x => x.Lines)
         .WithOne(x => x.PurchaseBillEntryHeader)
         .HasForeignKey(x => x.BillEntryHeaderId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
