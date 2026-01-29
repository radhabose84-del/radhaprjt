using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations
{
    public class WDVDepreciationDetailConfiguration : IEntityTypeConfiguration<WDVDepreciationDetail>
    {
        public void Configure(EntityTypeBuilder<WDVDepreciationDetail> builder)
        {
            builder.ToTable("WDVDepreciationDetail", "FixedAsset");
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

            builder.Property(dg => dg.FinYear )                
              .HasColumnType("int")
              .IsRequired(); 

            builder.Property(dg => dg.StartDate)                
              .HasColumnType("datetimeoffset")
              .IsRequired(); 

            builder.Property(dg => dg.EndDate)                
              .HasColumnType("datetimeoffset")
              .IsRequired(); 

            builder.Property(dg => dg.AssetGroupId)                
              .HasColumnType("int")
              .IsRequired(); 
                // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.AssetGroup)
              .WithMany(ag => ag.WDVDepreciationDetail)
              .HasForeignKey(dg => dg.AssetGroupId)                
              .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(dg => dg.AssetSubGroupId)                
              .HasColumnType("int")
              .IsRequired(false); 
                // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.AssetSubGroup)
              .WithMany(ag => ag.WDVDepreciationDetail)
              .HasForeignKey(dg => dg.AssetSubGroupId)                
              .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(dg => dg.DepreciationPercentage)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 
            
            builder.Property(dg => dg.OpeningValue)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 
            
            builder.Property(dg => dg.LastYearAdditionalDepreciation)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 

            builder.Property(dg => dg.LessThan180DaysValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(dg => dg.MoreThan180DaysValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 
              
            builder.Property(dg => dg.DeletionValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 
              
            builder.Property(dg => dg.ClosingValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

            builder.Property(dg => dg.DepreciationValue)                
                .HasColumnType("decimal(18,3)")
                .IsRequired(); 

            builder.Property(dg => dg.AdditionalDepreciationValue)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 

            builder.Property(dg => dg.WDVDepreciationValue)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 

            builder.Property(dg => dg.AdditionalCarryForward)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 

            builder.Property(dg => dg.CapitalGainLossValue)                
              .HasColumnType("decimal(18,3)")
              .IsRequired(); 

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
