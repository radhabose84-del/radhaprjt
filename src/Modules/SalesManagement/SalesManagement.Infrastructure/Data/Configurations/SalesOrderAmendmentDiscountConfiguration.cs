using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderAmendmentDiscountConfiguration : IEntityTypeConfiguration<SalesOrderAmendmentDiscount>
    {
        public void Configure(EntityTypeBuilder<SalesOrderAmendmentDiscount> builder)
        {
            builder.ToTable("SalesOrderAmendmentDiscount", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.SalesOrderAmendmentHeaderId).HasColumnName("SalesOrderAmendmentHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SalesOrderDiscountId).HasColumnName("SalesOrderDiscountId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.DiscountMasterId).HasColumnName("DiscountMasterId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SlabTypeId).HasColumnName("SlabTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.PaymentTermId).HasColumnName("PaymentTermId").HasColumnType("int").IsRequired();
            builder.Property(t => t.DiscountSlabId).HasColumnName("DiscountSlabId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.DiscountRate).HasColumnName("DiscountRate").HasColumnType("decimal(18,3)").IsRequired(false);
            builder.Property(t => t.TotalDiscountValue).HasColumnName("TotalDiscountValue").HasColumnType("decimal(18,3)").IsRequired(false);

            // Same-module FK
            builder.HasOne(t => t.SalesOrderAmendmentHeader)
                .WithMany(h => h.SalesOrderAmendmentDiscounts)
                .HasForeignKey(t => t.SalesOrderAmendmentHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.SalesOrderAmendmentHeaderId);
            builder.HasIndex(t => t.SalesOrderDiscountId);
            builder.HasIndex(t => t.DiscountMasterId);
        }
    }
}
