using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class CustomerVisitProductConfiguration : IEntityTypeConfiguration<CustomerVisitProduct>
    {
        public void Configure(EntityTypeBuilder<CustomerVisitProduct> builder)
        {
            builder.ToTable("CustomerVisitProduct", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CustomerVisitId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnType("int")
                .IsRequired();

            // Same-module FK: CustomerVisitId → Sales.CustomerVisit
            builder.HasOne(t => t.CustomerVisit)
                .WithMany(h => h.CustomerVisitProducts)
                .HasForeignKey(t => t.CustomerVisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.CustomerVisitId)
                .HasDatabaseName("IX_CustomerVisitProduct_CustomerVisitId");

            builder.HasIndex(t => t.ItemId)
                .HasDatabaseName("IX_CustomerVisitProduct_ItemId");

            // Cross-module FK: ItemId → InventoryManagement — no DB constraint
        }
    }
}
