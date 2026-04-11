using InventoryManagement.Domain.Entities.Item;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item
{
    public class ItemCategoryModuleConfiguration : IEntityTypeConfiguration<ItemCategoryModule>
    {
        public void Configure(EntityTypeBuilder<ItemCategoryModule> builder)
        {
            builder.ToTable("ItemCategoryModule", "Inventory");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ItemCategoryId)
                .HasColumnName("ItemCategoryId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ModuleId)
                .HasColumnName("ModuleId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(b => b.ItemCategory)
                .WithMany(c => c.ItemCategoryModules)
                .HasForeignKey(b => b.ItemCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.ItemCategoryId);
            builder.HasIndex(b => b.ModuleId);
            builder.HasIndex(b => new { b.ItemCategoryId, b.ModuleId }).IsUnique();
        }
    }
}
