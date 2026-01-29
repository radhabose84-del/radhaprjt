

using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations
{
    public class DepreciationDetailConfiguration : IEntityTypeConfiguration<DepreciationDetails>
    {
        public void Configure(EntityTypeBuilder<DepreciationDetails> builder)
        {
                builder.ToTable("DepreciationDetail", "FixedAsset");
                builder.Ignore(b => b.IsActive);
                builder.Ignore(b => b.IsDeleted);
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

                builder.Property(dg => dg.Finyear )                
                .HasColumnType("int")
                .IsRequired(); 

                 builder.Property(dg => dg.StartDate)                
                .HasColumnType("datetimeoffset")
                .IsRequired(); 

                builder.Property(dg => dg.EndDate)                
                .HasColumnType("datetimeoffset")
                .IsRequired(); 

                builder.Property(dg => dg.DepreciationType)                
                .HasColumnType("int")
                .IsRequired(); 
                 // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.DepType)
                .WithMany(ag => ag.DepType)
                .HasForeignKey(dg => dg.DepreciationType)                
                .OnDelete(DeleteBehavior.Restrict); 
                
                builder.Property(dg => dg.AssetId)                
                .HasColumnType("int")
                .IsRequired(); 
                // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetMasterId)
                .WithMany(ag => ag.DepreciationDetails)
                .HasForeignKey(dg => dg.AssetId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.AssetGroupId)                
                .HasColumnType("int")
                .IsRequired(); 
                  // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.AssetGroup)
                .WithMany(ag => ag.DepreciationDetails)
                .HasForeignKey(dg => dg.AssetGroupId)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.AssetValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 
                
                builder.Property(dg => dg.CapitalizationDate)                
                .HasColumnType("datetimeoffset")
                .IsRequired(); 

                builder.Property(dg => dg.ResidualValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

                builder.Property(dg => dg.ExpiryDate)                
                .HasColumnType("datetimeoffset")
                .IsRequired(); 

                builder.Property(dg => dg.UsefulLifeDays)                
                .HasColumnType("int")
                .IsRequired(); 

                builder.Property(dg => dg.DaysOpening)                
                .HasColumnType("int")
                .IsRequired(); 
                
                builder.Property(dg => dg.DaysUsed)                
                .HasColumnType("int")
                .IsRequired(); 

                builder.Property(dg => dg.OpeningValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

                builder.Property(dg => dg.DepreciationValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

                builder.Property(dg => dg.ClosingValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

                builder.Property(dg => dg.NetValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0);

                builder.Property(dg => dg.DepreciationPeriod)                
                .HasColumnType("int")
                .IsRequired(); 
                  // Configure Foreign Key Relationship
                builder.HasOne(dg => dg.DepMiscType)
                .WithMany(ag => ag.DepreciationPeriod)
                .HasForeignKey(dg => dg.DepreciationPeriod)                
                .OnDelete(DeleteBehavior.Restrict); 

                builder.Property(dg => dg.DisposedDate)                
                .HasColumnType("datetimeoffset");                

                builder.Property(dg => dg.DisposalAmount)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(false); 

                builder.Property(c => c.IsLocked)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
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
