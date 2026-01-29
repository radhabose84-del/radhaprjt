using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetWarrantyConfiguration : IEntityTypeConfiguration<AssetWarranties>
    {
        public void Configure(EntityTypeBuilder<AssetWarranties> builder)
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

                builder.ToTable("AssetWarranty", "FixedAsset");
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
                .WithMany(ag => ag.AssetWarranty)
                .HasForeignKey(dg => dg.AssetId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.StartDate)
                .HasColumnType("date")                                
                .IsRequired();
                
                builder.Property(dg => dg.EndDate)
                .HasColumnType("date")                                
                .IsRequired();

                builder.Property(dg => dg.Period)
                .HasColumnType("int")
                .IsRequired(); 

                builder.Property(dg => dg.WarrantyType)
                .HasColumnType("int")
                .IsRequired(); 

                 builder.HasOne(dg => dg.MiscWarrantyTypes)
                .WithMany(mm => mm.WarrantyClaim) 
                .HasForeignKey(dg => dg.WarrantyType)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_WarrantyType_Misc");

                builder.Property(dg => dg.WarrantyProvider)
                .HasColumnType("varchar(250)")
                .IsRequired();                 

                builder.Property(dg => dg.Description)
                .HasColumnType("varchar(1000)");                

                builder.Property(dg => dg.ContactPerson)
                .HasColumnType("varchar(50)")
                .IsRequired(); 

                builder.Property(dg => dg.MobileNumber)
                .HasColumnType("varchar(10)")
                .IsRequired();

                builder.Property(dg => dg.Email)
                .HasColumnType("varchar(100)");                

                 builder.Property(ca => ca.Document)
                .HasColumnName("Document")
                .HasColumnType("nvarchar(255)")
                .IsRequired(false);

                 builder.Property(dg => dg.ServiceCountryId)
                .HasColumnType("int");                
                
                builder.Property(dg => dg.ServiceStateId)
                .HasColumnType("int");                

                builder.Property(dg => dg.ServiceCityId)
                .HasColumnType("int");                

                builder.Property(dg => dg.ServiceAddressLine1)
                .HasColumnType("varchar(250)")   
                .IsRequired(false);             

                 builder.Property(dg => dg.ServiceAddressLine2)
                .HasColumnType("varchar(250)");                

                builder.Property(dg => dg.ServicePinCode)
                .HasColumnType("varchar(10)");                

                builder.Property(dg => dg.ServiceContactPerson)
                .HasColumnType("varchar(50)")
                .IsRequired(false);                

                 builder.Property(dg => dg.ServiceMobileNumber)
                .HasColumnType("varchar(10)")
                .IsRequired();              

                builder.Property(dg => dg.ServiceEmail)
                .HasColumnType("varchar(100)")
                .IsRequired();
                
                builder.Property(dg => dg.ServiceClaimProcessDescription)
                .HasColumnType("varchar(1000)");
                
                builder.Property(dg => dg.ServiceLastClaimDate)
                .HasColumnType("date");
                builder.Property(dg => dg.ServiceClaimStatus)
                .HasColumnType("int");

                 builder.HasOne(dg => dg.MiscClaimStatus)
                .WithMany(mm => mm.WarrantyType) 
                .HasForeignKey(dg => dg.ServiceClaimStatus)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ClaimStatus_Misc");

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