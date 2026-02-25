using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetPurchase
{
    public class AssetPurchaseDetailsConfiguration : IEntityTypeConfiguration<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>
    {
        public void Configure(EntityTypeBuilder<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails> builder)
        {
            builder.ToTable("AssetPurchaseDetails", "FixedAsset");
              
              // Primary Key
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
                
             builder.Property(ac => ac.AssetId)
                .HasColumnName("AssetId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ac => ac.AssetSourceId)
                .HasColumnName("AssetSourceId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(b => b.BudgetType)   
                 .HasColumnName("BudgetType")             
                 .HasColumnType("varchar(50)");   

                builder.Property(b => b.OldUnitId)   
                 .HasColumnName("OldUnitId")             
                 .HasColumnType("nvarchar(10)")
                 .IsRequired();  

                builder.Property(b => b.VendorCode)   
                 .HasColumnName("VendorCode")             
                 .HasColumnType("nvarchar(20)")
                 .IsRequired(); 

                 builder.Property(b => b.VendorName)   
                 .HasColumnName("VendorName")             
                 .HasColumnType("nvarchar(200)")
                 .IsRequired(); 

                builder.Property(b => b.PoDate)
                .HasColumnName("PoDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

                builder.Property(b => b.PoNo)
                .HasColumnName("PoNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.PoSno)
                .HasColumnName("PoSno")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ItemCode)
                .HasColumnName("ItemCode")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            builder.Property(b => b.ItemName)
                .HasColumnName("ItemName")
                .HasColumnType("nvarchar(250)")
                .IsRequired();

            builder.Property(b => b.GrnNo)
                .HasColumnName("GrnNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.GrnSno)
                .HasColumnName("GrnSno")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.GrnDate)
                .HasColumnName("GrnDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.QcCompleted)
                .HasColumnName("QcCompleted")
                .HasColumnType("char(1)")
                .IsRequired();

            builder.Property(b => b.AcceptedQty)
                .HasColumnName("AcceptedQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(b => b.PurchaseValue)
                .HasColumnName("PurchaseValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(b => b.GrnValue)
                .HasColumnName("GrnValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(b => b.BillNo)
                .HasColumnName("BillNo")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            builder.Property(b => b.BillDate)
                .HasColumnName("BillDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.Uom)
                .HasColumnName("Uom")
                .HasColumnType("nvarchar(10)");

            builder.Property(b => b.BinLocation)
                .HasColumnName("BinLocation")
                .HasColumnType("nvarchar(50)");

            builder.Property(b => b.PjYear)
                .HasColumnName("PjYear")
                .HasColumnType("varchar(8)")
                 .IsRequired();

            builder.Property(b => b.PjDocId)
                .HasColumnName("PjDocId")
                .HasColumnType("nvarchar(20)")
                  .IsRequired();
                
            builder.Property(b => b.PjDocSr)
                .HasColumnName("PjDocSr")
                .HasColumnType("nvarchar(20)");

            builder.Property(b => b.PjDocNo)
                .HasColumnName("PjDocNo")
                .HasColumnType("int")
                .IsRequired();

           builder.Property(b => b.CapitalizationDate)
                .HasColumnName("CapitalizationDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // Relationships
            builder.HasOne(b => b.Asset)
                .WithMany(pu => pu.AssetPurchase)
                .HasForeignKey(b => b.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.AssetSource)
                .WithMany(pu => pu.AssetPurchase)
                .HasForeignKey(b => b.AssetSourceId)
                .OnDelete(DeleteBehavior.Restrict);       
        }
    }
}
