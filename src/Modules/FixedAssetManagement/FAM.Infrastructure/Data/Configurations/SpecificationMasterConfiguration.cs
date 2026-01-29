using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations
{
    public class SpecificationMasterConfiguration : IEntityTypeConfiguration<SpecificationMasters>
    {
        public void Configure(EntityTypeBuilder<SpecificationMasters> builder)
        {
              // ValueConverter for Status (enum to bit)
                var statusConverter = new ValueConverter<Status, bool>(
                    v => v == Status.Active,                    
                    v => v ? Status.Active : Status.Inactive    
                );
            // ValueConverter for IsDelete (enum to bit)
                var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                    v => v == IsDelete.Deleted,                 
                    v => v ? IsDelete.Deleted : IsDelete.NotDeleted
                );

                builder.ToTable("SpecificationMaster", "FixedAsset");
                // Primary Key
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(dg => dg.AssetGroupId)                
                .HasColumnType("int")
                .IsRequired(); 

                 // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetGroupMaster)
                .WithMany(ag => ag.SpecificationMaster)
                .HasForeignKey(dg => dg.AssetGroupId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.SpecificationName)                
                .HasColumnType("varchar(50)")
                .IsRequired(); 

                builder.Property(c => c.ISDefault)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired(); 


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