
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PurchaseLocalHeaderConfiguration : IEntityTypeConfiguration<PurchaseLocalHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseLocalHeader> b)
    {
        b.ToTable("PurchaseLocalHeader", "Purchase");
        b.HasKey(x => x.Id);


        //b.Property(x => x.TermDescription).HasMaxLength(500);
        b.Property(x => x.TermDescription)
        .IsUnicode(true)                  // keep/remove as needed
        .HasColumnType("nvarchar(max)");
        b.Property(x => x.DeliveryAddress).HasMaxLength(500);
        b.Property(x => x.BillingAddress).HasMaxLength(500);

        b.HasOne(x => x.PurchaseLocal)
        .WithMany(m => m.Headers)
        .HasForeignKey(x => x.PurchaseOrderId)
        .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.MiscIncoterms)
         .WithMany(m => m.PurchaseLocalHeaderIncoterms)
         .HasForeignKey(x => x.IncotermsId)
         .OnDelete(DeleteBehavior.NoAction);
         
        b.HasOne(x => x.MiscModeOfDispatch)
         .WithMany(m => m.PurchaseLocalHeaderMode)
         .HasForeignKey(x => x.ModeOfDispatchId)
         .OnDelete(DeleteBehavior.NoAction);
    }
}