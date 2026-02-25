using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class ItemTransactionsConfiguration : IEntityTypeConfiguration<ItemTransactions>
    {
        public void Configure(EntityTypeBuilder<ItemTransactions> builder)
        {
           builder.ToTable("SubStores", "Maintenance");
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

             builder.Property(ag => ag.TC)
                .HasColumnName("TC")
                .HasColumnType("int")
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
                .HasColumnType("nvarchar(500)")
                .IsRequired();  

                builder.Property(dg => dg.Quantity)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired();

                builder.Property(dg => dg.Rate)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired();

                builder.Property(dg => dg.Value)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired();

                 builder.Property(ag => ag.CategoryDescription)
                .HasColumnName("CategoryDescription")
                .HasColumnType("nvarchar(100)");

                builder.Property(ag => ag.GroupName)
                .HasColumnName("GroupName")
                .HasColumnType("nvarchar(100)");

                 builder.Property(ag => ag.LifeType)
                .HasColumnName("LifeType")
                .HasColumnType("varchar(10)");

                builder.Property(ag => ag.LifeSpan)
                .HasColumnName("LifeSpan")
                .HasColumnType("int");

                 builder.Property(ag => ag.DepartmentName)
                .HasColumnName("DepartmentName")
                .HasColumnType("nvarchar(200)");


                builder.Property(b => b.CreatedDate)
                .HasColumnType("datetime")
                .IsRequired();
        }
    }
}