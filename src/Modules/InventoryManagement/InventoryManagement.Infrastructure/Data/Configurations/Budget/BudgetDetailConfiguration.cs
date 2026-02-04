using InventoryManagement.Domain.Entities.Budget;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Budget
{
    public class BudgetDetailConfiguration : IEntityTypeConfiguration<BudgetDetail>
    {
        public void Configure(EntityTypeBuilder<BudgetDetail> builder)
        {
            builder.ToTable("BudgetDetail", "Inventory");

            builder.HasKey(b => b.Id);      
            builder.Property(b => b.Id) 
                .HasColumnType("int")
                .IsRequired();             

            builder.Property(b => b.BudgetId)
                .HasColumnType("int")
                .IsRequired();    
            builder.HasOne(dg => dg.BudgetMaster)
                .WithMany(ag => ag.BudgetDetail)
                .HasForeignKey(dg => dg.BudgetId)                
                .OnDelete(DeleteBehavior.Restrict);         

            builder.Property(b => b.Month)
                .HasColumnType("int")
                .IsRequired();           

            builder.Property(b => b.BudgetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // ✅ Ignore unwanted BaseEntity properties
            builder.Ignore(b => b.IsActive);
            builder.Ignore(b => b.IsDeleted);

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");
    
            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(50)"); 

        }
    }
}
