using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BlanketPO;

public class PurchaseBlanketHeaderConfiguration : IEntityTypeConfiguration<PurchaseBlanketHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseBlanketHeader> b)
    {
        b.ToTable("PurchaseBlanketHeader", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.TermDescription)
            .IsUnicode(true)
            .HasColumnType("nvarchar(max)");
        b.Property(x => x.DeliveryAddress).HasMaxLength(500);
        b.Property(x => x.BillingAddress).HasMaxLength(500);
        b.Property(x => x.FreightCharges).HasPrecision(18, 2);

        // FK — PurchaseOrderId → PurchaseOrderHeader (1-to-1, UNIQUE)
        b.HasOne(x => x.PurchaseOrder)
            .WithMany(m => m.BlanketPOHeaders)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.PurchaseOrderId).IsUnique();

        // FK — BlanketHeaderId → BlanketHeader (same-module, no cascade)
        b.HasOne(x => x.BlanketHeader)
            .WithMany()
            .HasForeignKey(x => x.BlanketHeaderId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — IncotermsId → MiscMaster
        b.HasOne(x => x.MiscIncoterms)
            .WithMany(m => m.PurchaseBlanketHeaderIncoterms)
            .HasForeignKey(x => x.IncotermsId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — ModeOfDispatchId → MiscMaster
        b.HasOne(x => x.MiscModeOfDispatch)
            .WithMany(m => m.PurchaseBlanketHeaderMode)
            .HasForeignKey(x => x.ModeOfDispatchId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
