using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class StockLedgerConfiguration : IEntityTypeConfiguration<StockLedger>
    {
        public void Configure(EntityTypeBuilder<StockLedger> builder)
        {
             builder.ToTable("StockLedger", "Maintenance");
                // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.OldUnitCode)
                .HasColumnName("OldUnitCode")
                .HasColumnType("varchar(10)")
                .IsRequired();  

            builder.Property(ag => ag.TransactionType)
                .HasColumnName("TransactionType")
                .HasColumnType("varchar(50)")
                .IsRequired();  
            
            builder.Property(ag => ag.DocNo)
                .HasColumnName("DocNo")
                .HasColumnType("int")
                .IsRequired(); 

            builder.Property(ag => ag.DocSNo)
                .HasColumnName("DocSNo")
                .HasColumnType("int")
                .IsRequired(); 

              builder.Property(b => b.DocDate)
                .HasColumnType("datetime")
                .IsRequired();
                

             builder.Property(ag => ag.ItemCode)
                .HasColumnName("ItemCode")
                .HasColumnType("nvarchar(100)")
                .IsRequired();  

            builder.Property(ag => ag.ItemName)
                .HasColumnName("ItemName")
                .HasColumnType("nvarchar(500)")
                .IsRequired();  

            builder.Property(ag => ag.UOM)
                .HasColumnName("UOM")
                .HasColumnType("nvarchar(10)")
                .IsRequired();  

            builder.Property(dg => dg.ReceivedQty)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired()
                 .HasDefaultValue(0.00m);

             builder.Property(dg => dg.ReceivedValue)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired()
                 .HasDefaultValue(0.00m);

            builder.Property(dg => dg.IssueQty)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired()
                 .HasDefaultValue(0.00m);

            builder.Property(dg => dg.IssueValue)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired()
                .HasDefaultValue(0.00m);

            builder.Property(b => b.CreatedDate)
                .HasColumnType("datetime")
                .IsRequired();

            
        }
    }
}