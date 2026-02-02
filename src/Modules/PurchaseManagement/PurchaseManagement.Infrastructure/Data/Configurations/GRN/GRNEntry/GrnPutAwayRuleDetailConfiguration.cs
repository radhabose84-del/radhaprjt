using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.GRN.GRNEntry
{
    public class GrnPutAwayRuleDetailConfiguration : IEntityTypeConfiguration<GrnPutAwayRule>
    {
    public void Configure(EntityTypeBuilder<GrnPutAwayRule> builder)
    {
      builder.ToTable("GrnPutAwayRule", "Purchase");
      // Primary Key
      builder.HasKey(m => m.Id);

      builder.Property(m => m.Id)
          .HasColumnName("Id")
          .HasColumnType("int")
          .IsRequired();

      builder.Property(b => b.PutAwayDate)
              .HasColumnName("PutAwayDate")
              .IsRequired()
              .HasColumnType("DatetimeOffset");

      builder.Property(m => m.GrnDetailId)
         .HasColumnName("GrnDetailId")
         .HasColumnType("int")
         .IsRequired();

      // Foreign Key Relationship
      builder.HasOne(m => m.GrnHeaderPutAwayDetailsMaster)
          .WithMany(t => t.GrnPutAwayDetails)
          .HasForeignKey(m => m.GrnDetailId) // Foreign Key property in PartyContact
          .OnDelete(DeleteBehavior.Restrict); //

      builder.Property(m => m.UnitId)
           .HasColumnName("UnitId")
           .HasColumnType("int")
           .IsRequired();

      builder.Property(dg => dg.QcAcceptedQtyPurchaseUom)
        .HasColumnType("decimal(18,3)")
        .IsRequired(true)
        .HasDefaultValue(0.000m); // Set default value to 0.00

      builder.Property(m => m.GrnId)
      .HasColumnName("GrnId")
      .HasColumnType("int")
      .IsRequired();

      builder.Property(m => m.PoId)
    .HasColumnName("PoId")
    .HasColumnType("int")
    .IsRequired();


      builder.Property(m => m.PoSlNoLocal)
    .HasColumnName("PoSlNoLocal")
    .HasColumnType("int")
    .IsRequired();


      builder.Property(m => m.ItemId)
    .HasColumnName("ItemId")
    .HasColumnType("int")
    .IsRequired();

      builder.Property(m => m.WarehouseId)
     .HasColumnName("WarehouseId")
     .HasColumnType("int")
     .IsRequired();

      builder.Property(m => m.StorageTypeId)
     .HasColumnName("StorageTypeId")
     .HasColumnType("int")
     .IsRequired();

      builder.Property(m => m.TargetId)
           .HasColumnName("TargetId")
           .HasColumnType("int")
           .IsRequired();

      builder.Property(m => m.PriorityId)
           .HasColumnName("PriorityId")
           .HasColumnType("int")
           .IsRequired();

      builder.Property(t => t.Override)
      .HasColumnName("Override")
      .HasColumnType("bit")
      .IsRequired();

      builder.Property(m => m.CreatedBy)
         .HasColumnName("CreatedBy")
         .HasColumnType("int")
         .IsRequired();

      builder.Property(b => b.CreatedDate)
              .HasColumnName("CreatedDate")
              .IsRequired()
              .HasColumnType("DatetimeOffset");

      builder.Property(b => b.CreatedByName)
              .IsRequired()
              .HasColumnType("varchar(50)");

      builder.Property(b => b.CreatedIP)
             .IsRequired()
             .HasColumnType("varchar(20)");

      builder.Property(m => m.StockUomId)
             .HasColumnName("StockUomId")
             .HasColumnType("int")
             .IsRequired();

      builder.Property(m => m.PurchaseUomId)
                 .HasColumnName("PurchaseUomId")
                 .HasColumnType("int")
                 .IsRequired();

         builder.Property(dg => dg.ConversionFactor)
              .HasColumnType("decimal(18,3)")
              .IsRequired(false)
              .HasDefaultValue(0.000m); // Set default value to 0.00

        builder.Property(dg => dg.QcAcceptedQtyStockUom)
        .HasColumnType("decimal(18,3)")
        .IsRequired()
        .HasDefaultValue(0.000m); // Set default value to 0.00

            
        }
        
    }
}