using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetAmcConfiguration : IEntityTypeConfiguration<AssetAmc>
    {
        public void Configure(EntityTypeBuilder<AssetAmc> builder)
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
            builder.ToTable("AssetAmc", "FixedAsset");
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
            builder.HasOne(dg => dg.AssetMasterAmcId)
                .WithMany(ag => ag.AssetAmc)
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

            builder.Property(dg => dg.VendorCode)
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            builder.Property(dg => dg.VendorName)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            builder.Property(dg => dg.VendorEmail)
                .HasColumnType("nvarchar(100)");
           

            builder.Property(dg => dg.VendorPhone)
                .HasColumnType("nvarchar(40)");
             
            builder.Property(dg => dg.CoverageType)
                .HasColumnType("int")
                .IsRequired(); 

             // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.CoverageMiscType)
                .WithMany(ag => ag.AssetAmcCoverageType)
                .HasForeignKey(dg => dg.CoverageType)                
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(dg => dg.FreeServiceCount)
                .HasColumnType("int");

            builder.Property(dg => dg.RenewalStatus)
                .HasColumnType("int")
                .IsRequired();

               // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.RenewalStatusMiscType)
                .WithMany(ag => ag.AssetAmcRenewStatus)
                .HasForeignKey(dg => dg.RenewalStatus)                
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.RenewedDate)
                .HasColumnType("date")
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