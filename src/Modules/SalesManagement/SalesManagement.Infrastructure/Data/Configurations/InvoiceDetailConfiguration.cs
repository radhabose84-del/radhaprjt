using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
            builder.ToTable("InvoiceDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceHeaderId).HasColumnName("InvoiceHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemSno).HasColumnName("ItemSno").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.HsnCode).HasColumnName("HsnCode").HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.GstPercentage).HasColumnName("GstPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.LotId).HasColumnName("LotId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.NoOfBags).HasColumnName("NoOfBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.RatePerKg).HasColumnName("RatePerKg").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Discount).HasColumnName("Discount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxableAmount).HasColumnName("TaxableAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.CgstPercentage).HasColumnName("CgstPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SgstPercentage).HasColumnName("SgstPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IgstPercentage).HasColumnName("IgstPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.CGST).HasColumnName("CGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnName("SGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnName("IGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxAmount).HasColumnName("TaxAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.PackTypeId).HasColumnName("PackTypeId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.UOMId).HasColumnName("UOMId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,6)").IsRequired();

            // Cross-module FKs (PackType, LotMaster) → ProductionManagement — no DB constraint

            // Indexes
            builder.HasIndex(t => t.InvoiceHeaderId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.PackTypeId);
            builder.HasIndex(t => t.LotId);
        }
    }
}
