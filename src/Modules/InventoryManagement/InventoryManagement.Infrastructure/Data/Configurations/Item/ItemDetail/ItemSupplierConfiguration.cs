using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemSupplierConfiguration : IEntityTypeConfiguration<ItemSupplier>
    {
        public void Configure(EntityTypeBuilder<ItemSupplier> b)
        {
            b.ToTable("ItemSupplier", "Inventory");

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
             .WithMany(i => i.Suppliers)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.SupplierId)
             .HasColumnName("SupplierId")
             .HasColumnType("int")
             .IsRequired();

            b.Property(x => x.UnitId)
             .HasColumnName("UnitId")
             .HasColumnType("int")
             .IsRequired();

            b.Property(x => x.SupplierPartNo)
             .HasColumnName("SupplierPartNo")
             .HasColumnType("varchar(100)")
             .IsRequired(false);

            b.Property(x => x.LeadTime)
             .HasColumnName("LeadTime")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.MOQ)
             .HasColumnName("MOQ")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.MOQUomId)
             .HasColumnName("MOQUomId")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.PackageValue)
             .HasColumnName("PackageValue")
             .HasColumnType("decimal(18, 3)")
             .IsRequired(false);

            b.Property(x => x.PackageUomId)
             .HasColumnName("PackageUomId")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.DefaultSupplier)
             .HasColumnName("DefaultSupplier")
             .HasColumnType("bit")
             .IsRequired(false);        
        }
    }
}
