using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class ProductionStockLedgerConfiguration : IEntityTypeConfiguration<ProductionStockLedger>
    {
        public void Configure(EntityTypeBuilder<ProductionStockLedger> builder)
        {
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

            builder.ToTable("ProductionStockLedger", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnName("LotId").HasColumnType("int").IsRequired();

            builder.Property(t => t.DocDate).HasColumnName("DocDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();

            builder.Property(t => t.OpeningLooseKgs).HasColumnName("OpeningLooseKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ProdKgs).HasColumnName("ProdKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalProdKgs).HasColumnName("TotalProdKgs").HasColumnType("decimal(18,3)").IsRequired();

            builder.Property(t => t.PackTypeId).HasColumnName("PackTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeightPerPack).HasColumnName("NetWeightPerPack").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalBags).HasColumnName("TotalBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeight).HasColumnName("NetWeight").HasColumnType("decimal(18,3)").IsRequired();

            builder.Property(t => t.BagsRepacked).HasColumnName("BagsRepacked").HasColumnType("int").IsRequired();
            builder.Property(t => t.RepackKgs).HasColumnName("RepackKgs").HasColumnType("decimal(18,3)").IsRequired();

            builder.Property(t => t.ClosingLooseKgs).HasColumnName("ClosingLooseKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ClosingPackKgs).HasColumnName("ClosingPackKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ClosingBags).HasColumnName("ClosingBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.StockClosing).HasColumnName("StockClosing").HasColumnType("bit").HasDefaultValue(false).IsRequired();

            // Index for running-balance queries (latest entry per unit+item+lot)
            builder.HasIndex(t => new { t.UnitId, t.ItemId, t.LotId, t.DocDate, t.Id });
        }
    }
}
