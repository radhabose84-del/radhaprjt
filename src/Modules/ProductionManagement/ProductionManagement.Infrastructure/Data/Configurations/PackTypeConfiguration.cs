using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class PackTypeConfiguration : IEntityTypeConfiguration<PackType>
    {
        public void Configure(EntityTypeBuilder<PackType> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active, v => v ? Status.Active : Status.Inactive);
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted, v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("PackType", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.PackTypeCode).HasColumnName("PackTypeCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.PackTypeName).HasColumnName("PackTypeName").HasColumnType("varchar(100)").IsRequired();
            builder.Property(t => t.NetWeight).HasColumnName("NetWeight").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TareWeight).HasColumnName("TareWeight").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.GrossWeight).HasColumnName("GrossWeight").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.ConesPerBag).HasColumnName("ConesPerBag").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.PackMaterialId).HasColumnName("PackMaterialId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ProductionAllowed).HasColumnName("ProductionAllowed").HasColumnType("bit").HasDefaultValue(true).IsRequired();
            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasIndex(t => t.PackTypeCode).IsUnique();
            builder.HasIndex(t => t.PackMaterialId);

            // FK: PackType → MiscMaster (same-module, PackMaterialId)
            builder.HasOne(t => t.PackMaterial)
                .WithMany(m => m.PackTypesAsPackMaterial)
                .HasForeignKey(t => t.PackMaterialId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
