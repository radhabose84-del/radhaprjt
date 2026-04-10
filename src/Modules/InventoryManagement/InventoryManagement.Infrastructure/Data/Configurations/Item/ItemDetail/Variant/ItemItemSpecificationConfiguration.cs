using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Variant
{
    public sealed class ItemItemSpecificationConfiguration : IEntityTypeConfiguration<ItemItemSpecification>
    {
        public void Configure(EntityTypeBuilder<ItemItemSpecification> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("ItemItemSpecification", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.ItemId)
                .IsRequired();

            b.Property(x => x.SpecificationValueId)
                .IsRequired();

            b.Property(x => x.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            b.Property(x => x.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            b.HasOne(x => x.ItemMaster)
                .WithMany(i => i.ItemSpecifications)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.SpecificationValue)
                .WithMany(v => v.ItemSpecifications)
                .HasForeignKey(x => x.SpecificationValueId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.ItemId, x.SpecificationValueId }).IsUnique();
        }
    }
}
