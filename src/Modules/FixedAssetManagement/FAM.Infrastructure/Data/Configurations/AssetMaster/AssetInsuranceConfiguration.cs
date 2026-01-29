using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetInsuranceConfiguration : IEntityTypeConfiguration<AssetInsurance>
    {
       public void Configure(EntityTypeBuilder<AssetInsurance> builder)
        {


            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

                // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            
           builder.ToTable("AssetInsurance", "FixedAsset");

            builder.HasKey(al => al.Id); // Primary Key            

            builder.HasOne(ai => ai.AssetMaster)  
               .WithMany(amg => amg.AssetInsurance)  // Change to WithOne() if it's a One-to-One relationship
               .HasForeignKey(ai => ai.AssetId)  
               .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            builder.Property(ai => ai.PolicyNo)               
               .HasColumnType("nvarchar(50)")
               .IsRequired();
              
            builder.Property(ai => ai.StartDate)
               .HasColumnType("date")
               .IsRequired();

            builder.Property(ai => ai.Insuranceperiod)
                    .HasColumnType("int")
                    .IsRequired();

            builder.Property(ai => ai.EndDate)
               .HasColumnType("date")
               .IsRequired();
               
            builder.Property(ai => ai.PolicyAmount)
                .HasColumnType("decimal(18,3)") // Defines precision and scale
                .IsRequired(false);

            builder.Property(ai => ai.VendorCode)
                .HasColumnType("nvarchar(50)")
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(ai => ai.RenewalStatus)  
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ai => ai.RenewedDate)
                .HasColumnType("date")
                .IsRequired();                          

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

          builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

        builder.Property(b => b.CreatedBy)        
                .HasColumnType("int")
                .IsRequired();

        builder.Property(b => b.CreatedDate)        
                .HasColumnType("datetimeoffset")                       
                .IsRequired();    

        builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

    
        builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

        builder.Property(b => b.ModifiedBy)        
                .HasColumnType("int");
        

        builder.Property(b => b.ModifiedDate)        
                .HasColumnType("datetimeoffset");  

        builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

        builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)"); 

          


        }

        
    }
}