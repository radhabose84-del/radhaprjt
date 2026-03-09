using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class EInvoiceDetailConfiguration : IEntityTypeConfiguration<EInvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<EInvoiceDetail> builder)
        {
            builder.ToTable("EInvoiceDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.EInvoiceHeaderId).HasColumnName("EInvoiceHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemSno).HasColumnName("ItemSno").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemName).HasColumnName("ItemName").HasColumnType("varchar(200)").IsRequired(false);
            builder.Property(t => t.HsnNo).HasColumnName("HsnNo").HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.NoOfBags).HasColumnName("NoOfBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.Qty).HasColumnName("Qty").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.UnitPrice).HasColumnName("UnitPrice").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Rate).HasColumnName("Rate").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Discount).HasColumnName("Discount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxableAmount).HasColumnName("TaxableAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.GstPercentage).HasColumnName("GstPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.CGST).HasColumnName("CGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnName("SGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnName("IGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.OtherCharges).HasColumnName("OtherCharges").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.PackTypeId).HasColumnName("PackTypeId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.UOM).HasColumnName("UOM").HasColumnType("varchar(20)").IsRequired(false);

            // Indexes
            builder.HasIndex(t => t.EInvoiceHeaderId);
            builder.HasIndex(t => t.ItemId);
        }
    }
}
