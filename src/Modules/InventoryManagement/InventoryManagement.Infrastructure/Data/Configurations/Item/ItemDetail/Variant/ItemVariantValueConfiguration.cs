using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Variant
{
    public sealed class ItemVariantValueConfiguration : IEntityTypeConfiguration<ItemVariantValue>
    {
        public void Configure(EntityTypeBuilder<ItemVariantValue> b)
        {
            b.ToTable("ItemVariantValue", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.VariantAttributeId).IsRequired();
            b.Property(x => x.SpecificationValueId).IsRequired();

            b.HasOne(x => x.ItemMaster)
                .WithMany(i => i.VariantValues)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.VariantAttribute)
                .WithMany(a => a.ItemVariantValues)
                .HasForeignKey(x => x.VariantAttributeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.SpecificationValue)
                .WithMany(v => v.VariantValues)
                .HasForeignKey(x => x.SpecificationValueId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ParentItem)
                .WithMany(i => i.VariantParentItem)
                .HasForeignKey(x => x.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.ItemId, x.VariantAttributeId, x.SpecificationValueId }).IsUnique();
            b.HasIndex(x => new { x.ItemId, x.VariantAttributeId })
                .IsUnique()
                .HasDatabaseName("UX_ItemVariantValue_Item_VarAttr");
            b.HasIndex(x => x.ParentItemId);
        }
    }
}
