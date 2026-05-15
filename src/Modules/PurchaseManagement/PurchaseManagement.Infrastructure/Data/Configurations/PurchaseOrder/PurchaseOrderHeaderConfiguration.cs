using PurchaseManagement.Domain.Entities.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PurchaseOrderHeaderConfiguration : IEntityTypeConfiguration<PurchaseOrderHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderHeader> b)
    {
        b.ToTable("PurchaseOrderHeader", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.PONumber).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.PONumber).IsUnique();
        b.Property(x => x.ItemTotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountTotal).HasPrecision(18, 2);
        b.Property(x => x.PandFTotal).HasPrecision(18, 2);
        b.Property(x => x.MiscCharges).HasPrecision(18, 2);
        b.Property(x => x.GSTTotal).HasPrecision(18, 2);
        b.Property(x => x.CGSTTotal).HasPrecision(18, 2);
        b.Property(x => x.SGSTTotal).HasPrecision(18, 2);
        b.Property(x => x.IGSTTotal).HasPrecision(18, 2);
        b.Property(x => x.FreightTotal).HasPrecision(18, 2);
        b.Property(x => x.InsuranceTotal).HasPrecision(18, 2);
        b.Property(x => x.TDSTotal).HasPrecision(18, 2);
        b.Property(x => x.AdvanceAmount).HasPrecision(18, 2);
        b.Property(x => x.PurchaseValue).HasPrecision(18, 2);
        b.Property(e => e.CapitalTypeId).IsRequired(false);
        b.Property(e => e.CostCenterId).IsRequired(false);
        b.Property(e => e.ProjectId).IsRequired(false);
        b.Property(e => e.PurchaseTypeId).IsRequired(false);
        b.Property(e => e.ItemCategoryId).IsRequired(false);

        // Cancelled fields
        b.Property(x => x.CancelledDate).IsRequired(false);
        b.Property(x => x.CancelledByName).HasMaxLength(100).IsRequired(false);
        b.Property(x => x.CancelledIP).HasMaxLength(50).IsRequired(false);

        // ForeClosed fields
        b.Property(x => x.ForeClosedDate).IsRequired(false);
        b.Property(x => x.ForeClosedByName).HasMaxLength(100).IsRequired(false);
        b.Property(x => x.ForeClosedIP).HasMaxLength(50).IsRequired(false);

        b.HasOne(x => x.MiscPoCategory)
         .WithMany(m => m.PurchaseOrderCategory)
         .HasForeignKey(x => x.POCategoryId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.MiscPoMethod)
         .WithMany(m => m.PurchaseOrderMethod)
         .HasForeignKey(x => x.POMethodId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.MiscPurchaseType)
            .WithMany(m => m.POPurchaseType)
            .HasForeignKey(x => x.PurchaseTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.MiscCapitalType)
         .WithMany(m => m.POCapitalType)
         .HasForeignKey(x => x.CapitalTypeId)
         .OnDelete(DeleteBehavior.NoAction);
         
    }
}

