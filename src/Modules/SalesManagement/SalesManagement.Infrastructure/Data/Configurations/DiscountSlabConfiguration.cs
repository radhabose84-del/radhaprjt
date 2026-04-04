using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DiscountSlabConfiguration : IEntityTypeConfiguration<DiscountSlab>
    {
        public void Configure(EntityTypeBuilder<DiscountSlab> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DiscountSlab", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DiscountMasterId)
                .HasColumnName("DiscountMasterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SlabOrder)
                .HasColumnName("SlabOrder")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FromValue)
                .HasColumnName("FromValue")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.ToValue)
                .HasColumnName("ToValue")
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(t => t.DiscountValue)
                .HasColumnName("DiscountValue")
                .HasColumnType("decimal(18,4)")
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

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.DiscountMasterId);
            builder.HasIndex(t => new { t.DiscountMasterId, t.SlabOrder }).IsUnique();

            // FK: DiscountSlab → DiscountMaster (parent)
            builder.HasOne(t => t.DiscountMaster)
                .WithMany(d => d.DiscountSlabs)
                .HasForeignKey(t => t.DiscountMasterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
