using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StockLedgerConfiguration : IEntityTypeConfiguration<StockLedger>
    {
        public void Configure(EntityTypeBuilder<StockLedger> builder)
        {
            builder.ToTable("StockLedger", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocType)
                .HasColumnName("DocType")
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.DocNo)
                .HasColumnName("DocNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DetailDocNo)
                .HasColumnName("DetailDocNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocDate)
                .HasColumnName("DocDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LotId)
                .HasColumnName("LotId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PackNo)
                .HasColumnName("PackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PackTypeId)
                .HasColumnName("PackTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WarehouseId)
                .HasColumnName("WarehouseId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.BinId)
                .HasColumnName("BinId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalQty)
                .HasColumnName("TotalQty")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalValue)
                .HasColumnName("TotalValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            // Unique composite index — prevents duplicate pack entries
            builder.HasIndex(t => new { t.DocType, t.DocNo, t.PackNo }).IsUnique();

            // Indexes
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.WarehouseId);
            builder.HasIndex(t => t.DocDate);
        }
    }
}
