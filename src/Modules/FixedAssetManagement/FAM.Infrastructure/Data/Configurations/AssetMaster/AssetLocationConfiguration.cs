using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetLocationConfiguration : IEntityTypeConfiguration<AssetLocation>
    {
        public void Configure(EntityTypeBuilder<AssetLocation> builder)
        {
               builder.ToTable("AssetLocation", "FixedAsset");


                builder.HasKey(al => al.Id); // Primary Key

                // Foreign Key Relationships
                // Configure the foreign key relationship
               builder.HasOne(al => al.Asset)  
                .WithOne(a => a.AssetLocation)  
                .HasForeignKey<AssetLocation>(al => al.AssetId)  
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete


                builder.HasOne(al => al.Location)
                    .WithMany(l => l.AssetLocations)  // If Location has a collection, use .WithMany(l => l.AssetLocations)
                    .HasForeignKey(al => al.LocationId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete

                builder.HasOne(al => al.SubLocation)
                    .WithMany(l => l.AssetSubLocation)
                    .HasForeignKey(al => al.SubLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Other Required Constraints
                builder.Property(al => al.AssetId)
                  .HasColumnName("AssetId")
                   .HasColumnType("int")  // Set as int
                    .IsRequired();

                builder.Property(al => al.UnitId)
                 .HasColumnName("UnitId")
                   .HasColumnType("int")  // Set as int
                    .IsRequired();

                builder.Property(al => al.DepartmentId)
                     .HasColumnName("DepartmentId")
                   .HasColumnType("int")  // Set as int
                    .IsRequired();

                builder.Property(al => al.CustodianId)
                     .HasColumnName("CustodianId")
               .HasColumnType("int")  // Set as int 
                    .IsRequired();

                builder.Property(al => al.UserID)
                     .HasColumnName("UserId")
                    .HasColumnType("int")
                    .HasDefaultValue(0);  // Set as int           



        }
    }
}