using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class MovementTypeConfigConfiguration : IEntityTypeConfiguration<MovementTypeConfig>
    {
        public void Configure(EntityTypeBuilder<MovementTypeConfig> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("MovementTypeConfig", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MovementCode)
                .HasColumnName("MovementCode")
                .HasColumnType("varchar(4)")
                .IsRequired();

            builder.Property(t => t.MovementDescription)
                .HasColumnName("MovementDescription")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.MovementCategoryId)
                .HasColumnName("MovementCategoryId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FromStockTypeId)
                .HasColumnName("FromStockTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ToStockTypeId)
                .HasColumnName("ToStockTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QuantityUpdateFlag)
                .HasColumnName("QuantityUpdateFlag")
                .HasColumnType("bit")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(t => t.ValueUpdateFlag)
                .HasColumnName("ValueUpdateFlag")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.AccountModifier)
                .HasColumnName("AccountModifier")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.BatchRequiredFlag)
                .HasColumnName("BatchRequiredFlag")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.NegativeStockAllowed)
                .HasColumnName("NegativeStockAllowed")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.MovementCode).IsUnique();
            builder.HasIndex(t => t.MovementCategoryId);
            builder.HasIndex(t => t.FromStockTypeId);
            builder.HasIndex(t => t.ToStockTypeId);

            // FK: MovementTypeConfig → MiscMaster (MovementCategory) — same module
            builder.HasOne(t => t.MovementCategory)
                .WithMany(m => m.MovementTypeConfigsAsMovementCategory)
                .HasForeignKey(t => t.MovementCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: MovementTypeConfig → MiscMaster (FromStockType) — same module, StockType rows
            builder.HasOne(t => t.FromStockType)
                .WithMany(m => m.MovementTypeConfigsAsFromStockType)
                .HasForeignKey(t => t.FromStockTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: MovementTypeConfig → MiscMaster (ToStockType) — same module, StockType rows
            builder.HasOne(t => t.ToStockType)
                .WithMany(m => m.MovementTypeConfigsAsToStockType)
                .HasForeignKey(t => t.ToStockTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
