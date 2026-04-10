using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class PriceGroupMasterConfiguration : IEntityTypeConfiguration<PriceGroupMaster>
    {
        public void Configure(EntityTypeBuilder<PriceGroupMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("PriceGroupMaster", "Inventory");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PriceGroupCode)
                .HasColumnName("PriceGroupCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.PriceGroupName)
                .HasColumnName("PriceGroupName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(255)");

            builder.Property(t => t.EffectiveFrom)
                .HasColumnName("EffectiveFrom")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(t => t.EffectiveTo)
                .HasColumnName("EffectiveTo")
                .HasColumnType("datetimeoffset");

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");

            // ⭐ Soft-delete-safe unique indexes (CLAUDE.md §5 / SalesChannel bug fix).
            // Without HasFilter, soft-deleted rows would still occupy the unique slot,
            // blocking re-creation of a code/name that the user can no longer see.
            builder.HasIndex(p => p.PriceGroupCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(p => p.PriceGroupName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Non-unique helper index — no filter needed (not a constraint).
            builder.HasIndex(p => p.EffectiveFrom);
        }
    }
}
