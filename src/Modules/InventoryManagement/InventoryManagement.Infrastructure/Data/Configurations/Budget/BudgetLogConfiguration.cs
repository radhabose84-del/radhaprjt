using InventoryManagement.Domain.Entities.Budget;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Budget
{
    public class BudgetLogConfiguration : IEntityTypeConfiguration<BudgetLog>
    {
        public void Configure(EntityTypeBuilder<BudgetLog> builder)
        {
            builder.ToTable("BudgetLog", "Inventory");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id) 
                .HasColumnType("int")
                .IsRequired();            

            builder.Property(b => b.BudgetDetailId)
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(dg => dg.BudgetDetail)
                .WithMany(ag => ag.BudgetLog)
                .HasForeignKey(dg => dg.BudgetDetailId)                
                .OnDelete(DeleteBehavior.Restrict);     

            builder.Property(b => b.ActionTypeId)
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(dg => dg.MiscAction)
                .WithMany(ag => ag.BudgetAction)
                .HasForeignKey(dg => dg.ActionTypeId)                
                .OnDelete(DeleteBehavior.Restrict);  

            builder.Property(b => b.OldBudgetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(b => b.NewBudgetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.Remarks)
                .HasColumnType("varchar(1000)")
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
