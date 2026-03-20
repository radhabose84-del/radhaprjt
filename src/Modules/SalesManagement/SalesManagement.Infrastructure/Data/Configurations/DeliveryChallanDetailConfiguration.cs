using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DeliveryChallanDetailConfiguration : IEntityTypeConfiguration<DeliveryChallanDetail>
    {
        public void Configure(EntityTypeBuilder<DeliveryChallanDetail> builder)
        {
            builder.ToTable("DeliveryChallanDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DeliveryChallanHeaderId)
                .HasColumnName("DeliveryChallanHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoDetailId)
                .HasColumnName("StoDetailId")
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

            builder.Property(t => t.UOMId)
                .HasColumnName("UOMId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.BagCount)
                .HasColumnName("BagCount")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.BaleCount)
                .HasColumnName("BaleCount")
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

            builder.Property(t => t.ExMillRate)
                .HasColumnName("ExMillRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.LineMovementValue)
                .HasColumnName("LineMovementValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Same-module FK: DeliveryChallanDetail → StoDetail
            builder.HasOne(t => t.StoDetail)
                .WithMany(s => s.DeliveryChallanDetails)
                .HasForeignKey(t => t.StoDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // LotMaster → ProductionManagement — no DB constraint

            // Indexes
            builder.HasIndex(t => t.DeliveryChallanHeaderId);
            builder.HasIndex(t => t.StoDetailId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.LotId);
        }
    }
}
