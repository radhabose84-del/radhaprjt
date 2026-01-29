using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetSpecificationConfiguration : IEntityTypeConfiguration<AssetSpecifications>
    {
         public void Configure(EntityTypeBuilder<AssetSpecifications> builder)
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

                builder.ToTable("AssetSpecifications", "FixedAsset");
                // Primary Key
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(dg => dg.AssetId)                
                .HasColumnType("int")
                .IsRequired(); 
                 // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetMasterId)
                .WithMany(ag => ag.AssetSpecification)
                .HasForeignKey(dg => dg.AssetId)                
                .OnDelete(DeleteBehavior.Restrict); 

/*                 builder.Property(dg => dg.ManufactureId)                
                .HasColumnType("int")
                .IsRequired(false);
                
                // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.Manufacture)
                .WithMany(ag => ag.AssetSpecification)
                .HasForeignKey(dg => dg.ManufactureId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.ManufactureDate)                                
                .IsRequired(false);    */             
                    
                builder.Property(dg => dg.SpecificationId)
                .HasColumnType("int")
                .IsRequired(); 
                // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.SpecificationMaster)
                .WithMany(ag => ag.AssetSpecification)
                .HasForeignKey(dg => dg.SpecificationId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.SpecificationValue)                
                .HasColumnType("varchar(100)")
                .IsRequired();     

                /* builder.Property(dg => dg.SerialNumber)                
                .HasColumnType("varchar(100)")
                .IsRequired(false);  

                builder.Property(dg => dg.ModelNumber)                
                .HasColumnType("varchar(100)")
                .IsRequired(false);   */

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