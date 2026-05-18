using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ContractPO;

public class PurchaseContractDetailConfiguration : IEntityTypeConfiguration<PurchaseContractDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseContractDetail> b)
    {
        b.ToTable("PurchaseContractDetail", "Purchase");
        b.HasKey(x => x.Id);

        // Quantity/Price precision
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
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

        // FK — PurchaseContractHeaderId → PurchaseContractHeader (parent-child, cascade)
        b.HasOne(x => x.PurchaseContractHeader)
            .WithMany(m => m.Details)
            .HasForeignKey(x => x.PurchaseContractHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK — ContractPODetailId → ContractPODetail (links to contract line, no cascade)
        b.HasOne(x => x.ContractPODetail)
            .WithMany()
            .HasForeignKey(x => x.ContractPODetailId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — DiscountTypeId → MiscMaster
        b.HasOne(x => x.MiscDiscountType)
            .WithMany(m => m.PurchaseContractDetailDiscount)
            .HasForeignKey(x => x.DiscountTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        b.HasIndex(x => x.PurchaseContractHeaderId);
        b.HasIndex(x => x.ContractPODetailId);
    }
}
