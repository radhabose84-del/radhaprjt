using PurchaseManagement.Domain.Entities.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchasePaymentTermConfiguration : IEntityTypeConfiguration<PurchasePaymentTerm>
{
    public void Configure(EntityTypeBuilder<PurchasePaymentTerm> b)
    {
        b.ToTable("PurchasePaymentTerm", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.AdvanceAmount).HasPrecision(18, 2);
        b.Property(x => x.BalanceAmount).HasPrecision(18, 2);
        b.Property(x => x.InsuranceAmount).HasPrecision(18, 2);

        b.HasOne(x => x.PurchaseTerm)
        .WithMany(m => m.PaymentTerms)
        .HasForeignKey(x => x.PurchaseOrderId)
        .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MiscPOPaymentTerm)
         .WithMany(m => m.PurchaseOrderPaymentTerms)
         .HasForeignKey(x => x.PaymentTermId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.MiscPOPaymentMode)
         .WithMany(m => m.PurchaseOrderPaymentMode)
         .HasForeignKey(x => x.PaymentModelId)
         .OnDelete(DeleteBehavior.NoAction);
    }
}
