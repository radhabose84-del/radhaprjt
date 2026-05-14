using InventoryManagement.Domain.Entities.Item;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item
{
    public class ItemCategoryUnitConfigConfiguration : IEntityTypeConfiguration<ItemCategoryUnitConfig>
    {
        public void Configure(EntityTypeBuilder<ItemCategoryUnitConfig> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ItemCategoryUnitConfig", "Inventory");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ItemCategoryId)
                .HasColumnName("ItemCategoryId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(b => b.ItemCategory)
                .WithMany(c => c.UnitConfigs)
                .HasForeignKey(b => b.ItemCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(b => b.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.UOMId)
                .HasColumnName("UOMId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(b => b.UOM)
                .WithMany()
                .HasForeignKey(b => b.UOMId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.MaxSampleQuantity)
                .HasColumnName("MaxSampleQuantity")
                .HasColumnType("decimal(18,4)")
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(50)");

            builder.HasIndex(b => b.ItemCategoryId);
            builder.HasIndex(b => b.UnitId);
            builder.HasIndex(b => b.UOMId);
            builder.HasIndex(b => new { b.ItemCategoryId, b.UnitId })
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0");
        }
    }
}
