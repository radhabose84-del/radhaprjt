using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemUOMConfiguration : IEntityTypeConfiguration<ItemUOM>
    {
        public void Configure(EntityTypeBuilder<ItemUOM> b)
        {
            b.ToTable("ItemUOM", "Inventory");

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
             .WithMany(i => i.ItemUOMs)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.ConversionUOMId)
             .HasColumnName("ConversionUOMId")
             .HasColumnType("int")
             .IsRequired();
            b.HasOne(x => x.ConversionUOM)
            .WithMany(g => g.ItemUOM)              
             .HasForeignKey(x => x.ConversionUOMId)
             .OnDelete(DeleteBehavior.Restrict); 

            b.Property(x => x.ConversionRate)
             .HasColumnName("ConversionRate")
             .HasColumnType("decimal(18,6)")
             .IsRequired(false);
        }
    }
}
