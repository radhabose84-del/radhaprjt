using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Stock
{
    public class StockLedgerConfiguration : IEntityTypeConfiguration<InventoryManagement.Domain.Entities.Stock.StockLedger>
    {
        public void Configure(EntityTypeBuilder<InventoryManagement.Domain.Entities.Stock.StockLedger> builder)
        {
            builder.ToTable("StockLedger", "Inventory");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.DocType)
                .HasColumnName("DocType")
                .HasColumnType("varchar(50)")
                .IsRequired();


            builder.Property(ag => ag.DocNo)
                .HasColumnName("DocNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.DocSlNo)
                .HasColumnName("DocSlNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.DocDate)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(ag => ag.ItemId)
                .HasColumnName("ItemId")
                 .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.UomId)
                .HasColumnName("UomId")
                 .HasColumnType("int")
                .IsRequired();


            builder.Property(ag => ag.WarehouseId)
                .HasColumnName("WarehouseId")
                 .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.StorageTypeId)
                .HasColumnName("StorageTypeId")
                 .HasColumnType("int")
                .IsRequired();


            builder.Property(ag => ag.TargetId)
                .HasColumnName("TargetId")
                 .HasColumnType("int")
                .IsRequired();


            builder.Property(dg => dg.ReceivedQty)
               .HasColumnType("decimal(18,3)")
               .IsRequired(false)
                .HasDefaultValue(0.00m);


            builder.Property(dg => dg.ReceivedValue)
               .HasColumnType("decimal(18,3)")
               .IsRequired(false)
                .HasDefaultValue(0.00m);

            builder.Property(dg => dg.IssueQty)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false)
                 .HasDefaultValue(0.00m);

            builder.Property(dg => dg.IssueValue)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false)
                .HasDefaultValue(0.00m);
        }
    }
}