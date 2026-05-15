using InventoryManagement.Domain.Entities.Item;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item
{
    public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
    {
        public void Configure(EntityTypeBuilder<ItemCategory> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                    v => v == Status.Active,                    
                    v => v ? Status.Active : Status.Inactive    
                );            
                var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                    v => v == IsDelete.Deleted,                 
                    v => v ? IsDelete.Deleted : IsDelete.NotDeleted
                );
            builder.ToTable("ItemCategory", "Inventory");
                
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.ItemGroupId)
                .HasColumnName("ItemGroupId")
                .HasColumnType("int")
                .IsRequired();  
            builder.HasOne(dg => dg.ItemGroup)
                .WithMany(ag => ag.ItemCategory)
                .HasForeignKey(dg => dg.ItemGroupId)                
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(ag => ag.ItemCategoryName)
                .HasColumnName("ItemCategoryName")
                .HasColumnType("varchar(100)")
                .IsRequired(); 

            builder.Property(ag => ag.IsGroup)
                .HasColumnName("IsGroup")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);

            builder.Property(ag => ag.ParentCategoryId)
                .HasColumnName("ParentCategoryId")
                .HasColumnType("int")
                .IsRequired(false);
            builder.HasOne(dg => dg.ItemCategoryParent)
                .WithMany(ag => ag.ChildCategories)
                .HasForeignKey(dg => dg.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);           

            builder.Property(ag => ag.IsBudgetApplicable)
                .HasColumnName("IsBudgetApplicable")
                .HasColumnType("Bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(false);          
           
            builder.Property(ag => ag.EmergencyPoApplicable)
                .HasColumnName("EmergencyPoApplicable")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1,
                    v => v ? (byte)1 : (byte)0
                )
                .IsRequired(false);

            builder.Property(ag => ag.EmergencyPoLimit)
                .HasColumnName("EmergencyPoLimit")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(ag => ag.RootCategoryId)
                .HasColumnName("RootCategoryId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(ag => ag.DeptId)
                .HasColumnName("DeptId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(b => b.IsActive)                
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)                
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

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