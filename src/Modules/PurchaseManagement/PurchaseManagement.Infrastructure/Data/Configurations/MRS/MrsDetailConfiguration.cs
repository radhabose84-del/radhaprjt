using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.MRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.MRS
{
    public class MrsDetailConfiguration : IEntityTypeConfiguration<MrsDetail>
    {
        public void Configure(EntityTypeBuilder<MrsDetail> builder)
        {
            builder.ToTable("MrsDetail", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.MrsHeaderId)
                 .HasColumnName("MrsHeaderId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.MrsHeaderDetails)
                .WithMany(t => t.MrsDetailHeaderName)
                .HasForeignKey(m => m.MrsHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(m => m.ItemId)
                 .HasColumnName("ItemId")
                 .HasColumnType("int")
                 .IsRequired();


            builder.Property(m => m.UomId)
                 .HasColumnName("UomId")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(dg => dg.RequestQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(m => m.CostCenterId)
                 .HasColumnName("CostCenterId")
                 .HasColumnType("int")
                 .IsRequired(false);


            builder.Property(m => m.FinanceCode)
                 .HasColumnName("FinanceCode")
                 .HasColumnType("int")
                 .IsRequired(false);

              builder.Property(m => m.WarehouseStockId)
                 .HasColumnName("WarehouseStockId")
                 .HasColumnType("int")
                 .IsRequired();
             
        
        }
    }
}