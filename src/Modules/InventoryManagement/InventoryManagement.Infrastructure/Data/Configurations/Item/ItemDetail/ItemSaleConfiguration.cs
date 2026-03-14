using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemSaleConfiguration : IEntityTypeConfiguration<ItemSale>
    {
        public void Configure(EntityTypeBuilder<ItemSale> b)
        {
            b.ToTable("ItemSale", "Inventory");

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
             .WithOne(i => i.Sale)
             .HasForeignKey<ItemSale>(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.UomId)
             .HasColumnName("UomId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.SalesUOM)
             .WithMany(u => u.SalesUOM)
             .HasForeignKey(x => x.UomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.MinQuantity)
             .HasColumnName("MinQuantity")
             .HasColumnType("decimal(18,3)")
             .IsRequired();

            b.Property(x => x.PackageQuantity)
             .HasColumnName("PackageQuantity")
             .HasColumnType("decimal(18,3)")
             .IsRequired(false);

            b.Property(x => x.DeliveryLeadTime)
             .HasColumnName("DeliveryLeadTime")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.Discount)
             .HasColumnName("Discount")
             .HasColumnType("bit")
             .IsRequired();

            // CountId — cross-module FK to Production.CountMaster (no DB constraint)
            b.Property(x => x.CountId)
             .HasColumnName("CountId")
             .HasColumnType("int")
             .IsRequired(false);
        }
    }
}
