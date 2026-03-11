using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemPurchaseConfiguration : IEntityTypeConfiguration<ItemPurchase>
    {
        public void Configure(EntityTypeBuilder<ItemPurchase> b)
        {
            b.ToTable("ItemPurchase", "Inventory");

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
             .WithOne(i => i.Purchase)
             .HasForeignKey<ItemPurchase>(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.PurchaseUomId)
             .HasColumnName("PurchaseUomId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.PurchaseUOM)
             .WithMany(i => i.PurchaseUOM)
             .HasForeignKey(x => x.PurchaseUomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.LeadTimeDays)
             .HasColumnName("LeadTimeDays")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.GrProcessingTimeDays)
             .HasColumnName("GrProcessingTimeDays")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.AutomaticPo)
             .HasColumnName("AutomaticPo")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.SourceOfItem).HasColumnName("SourceOfItem").HasColumnType("int").IsRequired(false);
                b.HasOne(x => x.MiscSource)
                .WithMany(c => c.ItemPurchaseSource)
                .HasForeignKey(x => x.SourceOfItem)
                .OnDelete(DeleteBehavior.Restrict);    
        }
    }
}
