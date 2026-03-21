using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
    {
        public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
        {
            builder.ToTable("SalesOrderDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderHeaderId)
                .HasColumnName("SalesOrderHeaderId")
                .HasColumnType("int")
                .IsRequired();

            // Item Details
            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.VariantId)
                .HasColumnName("VariantId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.HSNId)
                .HasColumnName("HSNId")
                .HasColumnType("int")
                .IsRequired();

            // Quantity & Weight
            builder.Property(t => t.QtyInBags)
                .HasColumnName("QtyInBags")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.BagWeight)
                .HasColumnName("BagWeight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.SaleUOMId)
                .HasColumnName("SaleUOMId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalWeight)
                .HasColumnName("TotalWeight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Pricing
            builder.Property(t => t.ExMillRate)
                .HasColumnName("ExMillRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.DiscountPerUnit)
                .HasColumnName("DiscountPerUnit")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.Freight)
                .HasColumnName("Freight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Tax
            builder.Property(t => t.TaxableAmount)
                .HasColumnName("TaxableAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TaxPercentage)
                .HasColumnName("TaxPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TaxAmount)
                .HasColumnName("TaxAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TCSPercentage)
                .HasColumnName("TCSPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TCSAmount)
                .HasColumnName("TCSAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Computed
            builder.Property(t => t.NetAmount)
                .HasColumnName("NetAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.NetRatePerKg)
                .HasColumnName("NetRatePerKg")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Delivery & Tracking
            builder.Property(t => t.ExpectedDeliveryDate)
                .HasColumnName("ExpectedDeliveryDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.AgentCommissionPercentage)
                .HasColumnName("AgentCommissionPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.DispatchedQty)
                .HasColumnName("DispatchedQty")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PendingQty)
                .HasColumnName("PendingQty")
                .HasColumnType("int")
                .IsRequired();

            // Pack Type
            builder.Property(t => t.PackTypeId)
                .HasColumnName("PackTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            // PackType → ProductionManagement — no DB constraint

            // Status
            builder.Property(t => t.LineItemStatusId)
                .HasColumnName("LineItemStatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.HasOne(t => t.LineItemStatus)
                .WithMany(m => m.SalesOrderDetailsAsLineItemStatus)
                .HasForeignKey(t => t.LineItemStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.SalesOrderHeaderId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.HSNId);
            builder.HasIndex(t => t.PackTypeId);
        }
    }
}
