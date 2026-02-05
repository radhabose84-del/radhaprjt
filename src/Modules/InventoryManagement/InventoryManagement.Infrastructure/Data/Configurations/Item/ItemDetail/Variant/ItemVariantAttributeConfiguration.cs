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
            b.Property(x => x.AttributeId).IsRequired();       // FK -> MiscMaster.Id
            b.Property(x => x.VariantBasedOn).IsRequired();    // FK -> MiscMaster.Id
            b.Property(x => x.Order).IsRequired();

            b.HasOne(x => x.ItemMaster)
                .WithMany(i => i.VariantAttributes)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attribute (e.g., Dia, Size …) -> MiscMaster
            b.HasOne(x => x.MiscAttribute)
                .WithMany(m => m.ItemAttribute) // your collection in MiscMaster
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);

            // VariantBasedOn (Item Attribute / Manufacture) -> MiscMaster
            b.HasOne(x => x.MiscVariantBasedOn)
                .WithMany(m => m.ItemAttributeBasedOn) // your collection in MiscMaster
                .HasForeignKey(x => x.VariantBasedOn)
                .OnDelete(DeleteBehavior.Restrict);

            // AttributeGroup -> MiscTypeMaster
            b.HasOne(x => x.MiscAttributeGroup)
                .WithMany(t => t.ItemVariantAttributeGroup) // your collection in MiscTypeMaster
                .HasForeignKey(x => x.AttributeGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // optional: uniqueness and ordering guarantees per template
            b.HasIndex(x => new { x.ItemId, x.AttributeId }).IsUnique();     // only one row per attribute
            b.HasIndex(x => new { x.ItemId, x.Order }).IsUnique();           // unique order per item
        }
    }

}
