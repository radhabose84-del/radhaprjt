#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesGroupConfiguration : IEntityTypeConfiguration<SalesGroup>
    {
        public void Configure(EntityTypeBuilder<SalesGroup> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesGroup", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesGroupName)
                .HasColumnName("SalesGroupName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.SalesOfficeId)
                .HasColumnName("SalesOfficeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ResponsibleManager)
                .HasColumnName("ResponsibleManager")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.ProductCategoryId)
                .HasColumnName("ProductCategoryId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.RegionTerritory)
                .HasColumnName("RegionTerritory")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

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

            // Composite unique index: SalesGroupName unique within SalesOffice
            builder.HasIndex(t => new { t.SalesOfficeId, t.SalesGroupName }).IsUnique();
            builder.HasIndex(t => t.SalesOfficeId);

            // ProductCategoryId is cross-module (InventoryManagement) — no FK constraint
            builder.HasIndex(t => t.ProductCategoryId);

            // FK: SalesGroup → SalesOffice (same module, Sales schema)
            builder.HasOne(t => t.SalesOffice)
                .WithMany(o => o.SalesGroups)
                .HasForeignKey(t => t.SalesOfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
