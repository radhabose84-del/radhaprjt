using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemManufactureConfiguration : IEntityTypeConfiguration<ItemManufacture>
    {
        public void Configure(EntityTypeBuilder<ItemManufacture> b)
        {
          
            b.ToTable("ItemManufacture", "Inventory");

            // PK from BaseEntity
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
             .WithMany(i => i.Manufacture)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.UnitId)
             .HasColumnName("UnitId")
             .HasColumnType("int")
             .IsRequired();

            b.Property(x => x.ManufacturingTypeId)
             .HasColumnName("ManufacturingTypeId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.MiscManufactureType)             
             .WithMany(i => i.ItemManufactureType)
             .HasForeignKey(x => x.ManufacturingTypeId)
             .OnDelete(DeleteBehavior.Restrict);          
        }
    }
}
