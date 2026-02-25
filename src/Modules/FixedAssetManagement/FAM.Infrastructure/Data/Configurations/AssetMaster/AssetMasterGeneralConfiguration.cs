using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetMasterGeneralConfiguration : IEntityTypeConfiguration<AssetMasterGenerals>
    {
        public void Configure(EntityTypeBuilder<AssetMasterGenerals> builder)
        {
             var statusConverter = new ValueConverter<Status, bool>(
                    v => v == Status.Active,                    
                    v => v ? Status.Active : Status.Inactive    
                );
            // ValueConverter for IsDelete (enum to bit)
                var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                    v => v == IsDelete.Deleted,                 
                    v => v ? IsDelete.Deleted : IsDelete.NotDeleted
                );

                builder.ToTable("AssetMaster", "FixedAsset");
                // Primary Key
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(dg => dg.CompanyId)                
                .HasColumnType("int")
                .IsRequired(); 
                builder.Property(dg => dg.UnitId)                
                .HasColumnType("int")
                .IsRequired(); 

                builder.Property(dg => dg.AssetCode)                
                .HasColumnType("varchar(50)")
                .IsRequired();                
      
                builder.Property(dg => dg.AssetName)                
                .HasColumnType("varchar(100)")
                .IsRequired();                
              
                builder.Property(dg => dg.AssetGroupId)
                .HasColumnType("int")
                .IsRequired(); 
                // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetGroup)
                .WithMany(ag => ag.AssetMasterGeneral)
                .HasForeignKey(dg => dg.AssetGroupId)                
                .OnDelete(DeleteBehavior.Restrict);

                builder.Property(dg => dg.AssetSubGroupId)
                    .HasColumnType("int")       
                    .IsRequired(false);         
                    // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetSubGroup)
                    .WithMany(ag => ag.AssetMasterGeneral)
                    .HasForeignKey(dg => dg.AssetSubGroupId)                
                    .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.AssetCategoryId)
                .HasColumnType("int")
                .IsRequired(); 
                builder.HasOne(dg => dg.AssetCategories)
                .WithMany(ag => ag.AssetMasterGeneral)
                .HasForeignKey(dg => dg.AssetCategoryId)                
                .OnDelete(DeleteBehavior.Restrict); 
                
                builder.Property(dg => dg.AssetSubCategoryId)
                .HasColumnType("int")
                .IsRequired(); 
                builder.HasOne(dg => dg.AssetSubCategories)
                .WithMany(ag => ag.AssetMasterGeneral)
                .HasForeignKey(dg => dg.AssetSubCategoryId)                
                .OnDelete(DeleteBehavior.Restrict); 
                
                 builder.Property(dg => dg.AssetParentId)
                .HasColumnType("int")
                .IsRequired(false); 

                builder.HasOne(dg => dg.AssetParent)
                .WithMany(ag => ag.AssetChildren)
                .HasForeignKey(dg => dg.AssetParentId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cyclic deletion 

                builder.Property(b => b.AssetType)                
                .HasColumnType("int")
                .IsRequired(false);
                
                builder.HasOne(dg => dg.AssetMiscType)
                .WithMany(mm => mm.AssetMiscTypeGenerals) 
                .HasForeignKey(dg => dg.AssetType)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AssetType_Misc");

                 builder.Property(b => b.MachineCode)                
                .HasColumnType("varchar(50)");                
                
                 builder.Property(b => b.Quantity)                
                .HasColumnType("int")
                .IsRequired();

                builder.Property(ag => ag.UOMId)
                .HasColumnName("UOMId")
                .HasColumnType("int")
                .IsRequired(false); 
                builder.HasOne(dg => dg.UomMaster)
                .WithMany(mm => mm.AssetGeneralsUom) 
                .HasForeignKey(dg => dg.UOMId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AssetUOM_UOMMaster");

                builder.Property(b => b.WorkingStatus)
                .IsRequired(false)
                .HasColumnType("int");

                builder.HasOne(dg => dg.AssetWorkType)
                  .WithMany(mm => mm.AssetWorkTypeGenerals) 
                .HasForeignKey(dg => dg.WorkingStatus)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_WorkingStatus_Misc");                

                builder.Property(c => c.IsTangible)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

                builder.Property(c => c.ISDepreciated)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();
            
                builder.Property(b => b.AssetDescription)
                .IsRequired(false)
                .HasColumnType("varchar(250)");

                builder.Property(ca => ca.AssetImage)
                .HasColumnName("AssetImage")
                .HasColumnType("nvarchar(255)");

                builder.Property(ca => ca.AssetDocument)
                .HasColumnName("AssetDocument")
                .HasColumnType("nvarchar(255)");

                 builder.Property(b => b.PutToUseDate)
                .HasColumnName("PutToUseDate")
                .HasColumnType("datetimeoffset")
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