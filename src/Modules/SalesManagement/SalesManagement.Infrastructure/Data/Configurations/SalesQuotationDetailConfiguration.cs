using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesQuotationDetailConfiguration : IEntityTypeConfiguration<SalesQuotationDetail>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationDetail> builder)
        {
            builder.ToTable("SalesQuotationDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesQuotationHeaderId)
                .HasColumnName("SalesQuotationHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(t => t.SalesQuotationHeader)
                .WithMany(h => h.SalesQuotationDetails)
                .HasForeignKey(t => t.SalesQuotationHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Quantity)
                .HasColumnName("Quantity")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.ExMillRate)
                .HasColumnName("ExMillRate")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.Discount)
                .HasColumnName("Discount")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.NetRate)
                .HasColumnName("NetRate")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.TotalAmount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.HSNId)
                .HasColumnName("HSNId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TaxPercentage)
                .HasColumnName("TaxPercentage")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.TaxAmount)
                .HasColumnName("TaxAmount")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.SalesQuotationHeaderId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.HSNId);
        }
    }
}
