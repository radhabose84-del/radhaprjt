using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ContractPO;

public class PurchaseContractHeaderConfiguration : IEntityTypeConfiguration<PurchaseContractHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseContractHeader> b)
    {
        b.ToTable("PurchaseContractHeader", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.TermDescription)
            .IsUnicode(true)
            .HasColumnType("nvarchar(max)");
        b.Property(x => x.DeliveryAddress).HasMaxLength(500);
        b.Property(x => x.BillingAddress).HasMaxLength(500);
        b.Property(x => x.FreightCharges).HasPrecision(18, 2);

        // FK — PurchaseOrderId → PurchaseOrderHeader (1-to-1, UNIQUE)
        b.HasOne(x => x.PurchaseOrder)
            .WithMany(m => m.ContractPOHeaders)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.PurchaseOrderId).IsUnique();

        // FK — ContractPOHeaderId → ContractPOHeader (same-module, no cascade)
        b.HasOne(x => x.ContractPOHeader)
            .WithMany()
            .HasForeignKey(x => x.ContractPOHeaderId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — IncotermsId → MiscMaster
        b.HasOne(x => x.MiscIncoterms)
            .WithMany(m => m.PurchaseContractHeaderIncoterms)
            .HasForeignKey(x => x.IncotermsId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — ModeOfDispatchId → MiscMaster
        b.HasOne(x => x.MiscModeOfDispatch)
            .WithMany(m => m.PurchaseContractHeaderMode)
            .HasForeignKey(x => x.ModeOfDispatchId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
