using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Variant
{
    public sealed class ItemVariantAttributeConfiguration : IEntityTypeConfiguration<ItemVariantAttribute>
    {
        public void Configure(EntityTypeBuilder<ItemVariantAttribute> b)
        {
            b.ToTable("ItemVariantAttribute", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.SpecificationMasterId).IsRequired();
            b.Property(x => x.Order).IsRequired();

            b.HasOne(x => x.ItemMaster)
                .WithMany(i => i.VariantAttributes)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.SpecificationMaster)
                .WithMany(s => s.VariantAttributes)
                .HasForeignKey(x => x.SpecificationMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.ItemId, x.SpecificationMasterId }).IsUnique();
            b.HasIndex(x => new { x.ItemId, x.Order }).IsUnique();
        }
    }
}
