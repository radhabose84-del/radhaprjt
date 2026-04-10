using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Variant
{
    public sealed class ItemSpecificationMasterConfiguration : IEntityTypeConfiguration<ItemSpecificationMaster>
    {
        public void Configure(EntityTypeBuilder<ItemSpecificationMaster> b)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            b.ToTable("ItemSpecificationMaster", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.SpecificationCode)
                .HasColumnType("varchar(20)")
                .IsRequired();

            b.Property(x => x.SpecificationName)
                .HasColumnType("varchar(100)")
                .IsRequired();

            b.Property(x => x.Order)
                .IsRequired();

            b.Property(x => x.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            b.Property(x => x.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            b.HasIndex(x => x.SpecificationCode).IsUnique();
            b.HasIndex(x => x.Order).IsUnique();
        }
    }
}
