using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StoReceiptDetailConfiguration : IEntityTypeConfiguration<StoReceiptDetail>
    {
        public void Configure(EntityTypeBuilder<StoReceiptDetail> builder)
        {
            builder.ToTable("StoReceiptDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoReceiptHeaderId)
                .HasColumnName("StoReceiptHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DeliveryChallanDetailId)
                .HasColumnName("DeliveryChallanDetailId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LotId)
                .HasColumnName("LotId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StartPackNo)
                .HasColumnName("StartPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnName("EndPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchQuantity)
                .HasColumnName("DispatchQuantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ReceivedQuantity)
                .HasColumnName("ReceivedQuantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.DamageQuantity)
                .HasColumnName("DamageQuantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.AcceptedQuantity)
                .HasColumnName("AcceptedQuantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.UOMId)
                .HasColumnName("UOMId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.BagCount)
                .HasColumnName("BagCount")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.NetWeight)
                .HasColumnName("NetWeight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.GrossWeight)
                .HasColumnName("GrossWeight")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.LineStatusId)
                .HasColumnName("LineStatusId")
                .HasColumnType("int")
                .IsRequired();

            // Same-module FK: StoReceiptDetail → DeliveryChallanDetail
            builder.HasOne(t => t.DeliveryChallanDetail)
                .WithMany(dc => dc.StoReceiptDetails)
                .HasForeignKey(t => t.DeliveryChallanDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: StoReceiptDetail → LotMaster
            builder.HasOne(t => t.LotMaster)
                .WithMany(l => l.StoReceiptDetails)
                .HasForeignKey(t => t.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: StoReceiptDetail → MiscMaster (LineStatus)
            builder.HasOne(t => t.LineStatus)
                .WithMany(m => m.StoReceiptDetailsAsLineStatus)
                .HasForeignKey(t => t.LineStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.StoReceiptHeaderId);
            builder.HasIndex(t => t.DeliveryChallanDetailId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.LotId);
            builder.HasIndex(t => t.LineStatusId);
        }
    }
}
