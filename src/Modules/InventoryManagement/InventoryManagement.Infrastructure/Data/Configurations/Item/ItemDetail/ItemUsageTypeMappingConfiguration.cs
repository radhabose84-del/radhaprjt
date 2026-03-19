using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemUsageTypeMappingConfiguration : IEntityTypeConfiguration<ItemUsageTypeMapping>
    {
        public void Configure(EntityTypeBuilder<ItemUsageTypeMapping> b)
        {
            b.ToTable("ItemUsageTypeMapping", "Inventory");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
             .HasColumnName("Id")
             .HasColumnType("int")
             .UseIdentityColumn();

            b.Property(x => x.ItemId)
             .HasColumnName("ItemId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.Item)
             .WithMany(i => i.ItemUsageTypeMappings)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.UsageTypeId)
             .HasColumnName("UsageTypeId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.UsageType)
             .WithMany(u => u.ItemUsageTypeMappings)
             .HasForeignKey(x => x.UsageTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.UnitId)
             .HasColumnName("UnitId")
             .HasColumnType("int")
             .IsRequired();

            // Composite unique constraint on all 3 FK fields
            b.HasIndex(x => new { x.ItemId, x.UsageTypeId, x.UnitId })
             .IsUnique();
        }
    }
}
