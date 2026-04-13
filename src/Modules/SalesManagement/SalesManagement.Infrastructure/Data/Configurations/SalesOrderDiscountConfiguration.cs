using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderDiscountConfiguration : IEntityTypeConfiguration<SalesOrderDiscount>
    {
        public void Configure(EntityTypeBuilder<SalesOrderDiscount> builder)
        {
            builder.ToTable("SalesOrderDiscount", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderHeaderId)
                .HasColumnName("SalesOrderHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DiscountMasterId)
                .HasColumnName("DiscountMasterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SlabTypeId)
                .HasColumnName("SlabTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PaymentTermId)
                .HasColumnName("PaymentTermId")
                .HasColumnType("int")
                .IsRequired();

            // Same-module FK constraints
            builder.HasOne(t => t.SalesOrderHeader)
                .WithMany(h => h.SalesOrderDiscounts)
                .HasForeignKey(t => t.SalesOrderHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DiscountMaster)
                .WithMany(d => d.SalesOrderDiscounts)
                .HasForeignKey(t => t.DiscountMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SlabTypeMisc)
                .WithMany(m => m.SalesOrderDiscountsAsSlabType)
                .HasForeignKey(t => t.SlabTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.SalesOrderHeaderId);
            builder.HasIndex(t => t.DiscountMasterId);
            builder.HasIndex(t => t.SlabTypeId);
            builder.HasIndex(t => new { t.SalesOrderHeaderId, t.DiscountMasterId }).IsUnique();
        }
    }
}
