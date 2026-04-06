using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DiscountSalesGroupConfiguration : IEntityTypeConfiguration<DiscountSalesGroup>
    {
        public void Configure(EntityTypeBuilder<DiscountSalesGroup> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DiscountSalesGroup", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DiscountMasterId)
                .HasColumnName("DiscountMasterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesGroupId)
                .HasColumnName("SalesGroupId")
                .HasColumnType("int")
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

            // Composite unique index: prevent duplicate SalesGroup per DiscountMaster
            builder.HasIndex(t => new { t.DiscountMasterId, t.SalesGroupId }).IsUnique();
            builder.HasIndex(t => t.DiscountMasterId);
            builder.HasIndex(t => t.SalesGroupId);

            // FK: DiscountSalesGroup → DiscountMaster (parent)
            builder.HasOne(t => t.DiscountMaster)
                .WithMany(d => d.DiscountSalesGroups)
                .HasForeignKey(t => t.DiscountMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: DiscountSalesGroup → SalesGroup (same module)
            builder.HasOne(t => t.SalesGroup)
                .WithMany(g => g.DiscountSalesGroups)
                .HasForeignKey(t => t.SalesGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
