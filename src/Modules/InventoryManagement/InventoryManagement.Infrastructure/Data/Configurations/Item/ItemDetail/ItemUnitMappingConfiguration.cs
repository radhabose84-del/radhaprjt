using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemUnitMappingConfiguration : IEntityTypeConfiguration<ItemUnitMapping>
    {
        public void Configure(EntityTypeBuilder<ItemUnitMapping> b)
        {
            b.ToTable("ItemUnitMapping", "Inventory");

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
             .WithMany(i => i.ItemUnitMappings)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.ProcurementId)
             .HasColumnName("ProcurementId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.ProcurementType)
             .WithMany(p => p.ItemUnitMappings)
             .HasForeignKey(x => x.ProcurementId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.ItemGroupId)
             .HasColumnName("ItemGroupId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.ItemGroup)
             .WithMany(g => g.ItemUnitMappings)
             .HasForeignKey(x => x.ItemGroupId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.UnitId)
             .HasColumnName("UnitId")
             .HasColumnType("int")
             .IsRequired();

            // Composite unique constraint on all 4 FK fields
            b.HasIndex(x => new { x.ItemId, x.ProcurementId, x.ItemGroupId, x.UnitId })
             .IsUnique();
        }
    }
}
