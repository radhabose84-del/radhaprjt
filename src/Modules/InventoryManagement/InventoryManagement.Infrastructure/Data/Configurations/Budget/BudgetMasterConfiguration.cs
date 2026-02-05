using InventoryManagement.Domain.Entities.Budget;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Budget
{
    public class BudgetMasterConfiguration : IEntityTypeConfiguration<BudgetMaster>
    {
        public void Configure(EntityTypeBuilder<BudgetMaster> builder)
        {
            builder.ToTable("BudgetMaster", "Inventory");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id) 
                .HasColumnType("int")
                .IsRequired();       

            builder.Property(b => b.UnitId)
                .HasColumnType("int")
                .IsRequired();            

            builder.Property(b => b.BudgetGroupId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.FiscalYear)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.YearBudgetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            
            builder.Property(ag => ag.Is_MRApplicable)
                .HasColumnName("Is_MRApplicable")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);
            
            builder.Property(ag => ag.Is_POApplicable)
                .HasColumnName("Is_POApplicable")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);

            
            builder.Property(ag => ag.Is_ServiceApplicable)
                .HasColumnName("Is_ServiceApplicable")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);

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
