using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class ProductionPackDetailConfiguration : IEntityTypeConfiguration<ProductionPackDetail>
    {
        public void Configure(EntityTypeBuilder<ProductionPackDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active, v => v ? Status.Active : Status.Inactive);
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted, v => v ? IsDelete.Deleted : IsDelete.NotDeleted);
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

            builder.ToTable("ProductionPackDetail", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.PackNo).HasColumnName("PackNo").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.PackDate).HasColumnName("PackDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();
            builder.Property(t => t.ProductionYear).HasColumnName("ProductionYear").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.WarehouseId).HasColumnName("WarehouseId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnName("LotId").HasColumnType("int").IsRequired();
            builder.Property(t => t.PackTypeId).HasColumnName("PackTypeId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.NetWeightPerPack).HasColumnName("NetWeightPerPack").HasColumnType("decimal(18,3)").IsRequired(false);
            builder.Property(t => t.StartPackNo).HasColumnName("StartPackNo").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.EndPackNo).HasColumnName("EndPackNo").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.TotalBags).HasColumnName("TotalBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.TotalNetWeight).HasColumnName("TotalNetWeight").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ProductionKgs).HasColumnName("ProductionKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.LooseConeKgs).HasColumnName("LooseConeKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.BinId).HasColumnName("BinId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.QualityStatusId).HasColumnName("QualityStatusId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.StockClosing).HasColumnName("StockClosing").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("nvarchar(500)").IsRequired(false);
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

            // Same-module FK relationships
            builder.HasOne(t => t.LotMaster).WithMany().HasForeignKey(t => t.LotId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.PackType).WithMany().HasForeignKey(t => t.PackTypeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.QualityStatusMisc).WithMany().HasForeignKey(t => t.QualityStatusId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => new { t.PackNo, t.ProductionYear });
            builder.HasIndex(t => t.LotId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.PackDate);
        }
    }
}
