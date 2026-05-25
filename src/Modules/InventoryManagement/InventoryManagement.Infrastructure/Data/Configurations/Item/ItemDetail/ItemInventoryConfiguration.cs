using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemInventoryConfiguration : IEntityTypeConfiguration<ItemInventory>
    {
        public void Configure(EntityTypeBuilder<ItemInventory> b)
        {
            b.ToTable("ItemInventory", "Inventory");
            
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
             .WithOne(i => i.Inventory)
             .HasForeignKey<ItemInventory>(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            
            b.Property(x => x.Weight)
             .HasColumnName("Weight")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.WeightUomId)
             .HasColumnName("WeightUomId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.WeightUOM)
             .WithMany(g => g.InventoryUOM)
             .HasForeignKey(x => x.WeightUomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.Length)
             .HasColumnName("Length")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.Breadth)
             .HasColumnName("Breadth")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.Height)
             .HasColumnName("Height")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.Volume)
             .HasColumnName("Volume")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.DimensionUomId)
             .HasColumnName("DimensionUomId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.DimensionUOM)
             .WithMany(g => g.InventoryDimensionUOM)
             .HasForeignKey(x => x.DimensionUomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.DefaultMaterialRequestTypeId)
             .HasColumnName("DefaultMaterialRequestTypeId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.MiscDefaultMaterialRequestType)
             .WithMany(g => g.ItemInventoryDefaultMaterialRequestType) 
             .HasForeignKey(x => x.DefaultMaterialRequestTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.ValuationMethodId)
             .HasColumnName("ValuationMethodId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.MiscValuationMethod)
             .WithMany(g => g.ItemInventoryValuationMethod)              
             .HasForeignKey(x => x.ValuationMethodId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.ShelfLife)
             .HasColumnName("ShelfLife")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.UpperTolerance)
             .HasColumnName("UpperTolerance")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.LowerTolerance)
             .HasColumnName("LowerTolerance")
             .HasColumnType("decimal(18,4)")
             .IsRequired(false);

            b.Property(x => x.BatchNumberSeries)
             .HasColumnName("BatchNumberSeries")
             .HasColumnType("varchar(100)")
             .IsRequired(false);

            b.Property(x => x.SerialNumberSeries)
             .HasColumnName("SerialNumberSeries")
             .HasColumnType("varchar(100)")
             .IsRequired(false);

            b.Property(x => x.ReorderLevel)
             .HasColumnName("ReorderLevel")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.ReorderQty)
             .HasColumnName("ReorderQty")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.RequestTypeId)
             .HasColumnName("RequestTypeId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.MiscRequestType)
             .WithMany(g => g.ItemInventoryRequestType) 
             .HasForeignKey(x => x.RequestTypeId)
             .OnDelete(DeleteBehavior.Restrict); 

            b.Property(x => x.SafetyStock)
             .HasColumnName("SafetyStock")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.AllowNegativeStock)
             .HasColumnName("AllowNegativeStock")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.BatchManagement)
             .HasColumnName("BatchManagement")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.ApplyBatchNumber)
             .HasColumnName("ApplyBatchNumber")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.DefaultPackTypeId)
             .HasColumnName("DefaultPackTypeId")
             .HasColumnType("int")
             .IsRequired(false);
        }
    }
}
