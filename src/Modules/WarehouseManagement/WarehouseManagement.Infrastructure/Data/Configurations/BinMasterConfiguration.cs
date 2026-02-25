using WarehouseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.Infrastructure.Data.Configurations
{
    public class BinMasterConfiguration : IEntityTypeConfiguration<BinMaster>
    {

        public void Configure(EntityTypeBuilder<BinMaster> builder)
        {

            var statusConverter = new ValueConverter<Status, bool>(
                   v => v == Status.Active,                    
                   v => v ? Status.Active : Status.Inactive    
               );

           
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
               );

            builder.ToTable("WarehouseMaster", " BinMaster");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.WarehouseId)
                 .IsRequired();

            builder.Property(x => x.RackId);

            builder.Property(x => x.BinCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.BinName)
                   .HasMaxLength(50);

            builder.Property(x => x.BinCapacity)
                   .IsRequired()
                   .HasColumnType("decimal(18,3)");

            builder.Property(x => x.CapacityUOMId)
                   .IsRequired();

            builder.HasIndex(x => new { x.WarehouseId, x.BinCode })
                  .IsUnique()
                  .HasDatabaseName("UQ_Bin_Warehouse_BinCode");


            builder.ToTable("BinMaster", "Warehouse", t =>
                {
                    // check constraint inside ToTable
                    t.HasCheckConstraint("CK_BinCapacity_Positive", "[BinCapacity] > 0");
                });
                        
            builder.HasOne(x => x.Warehouse)
                   .WithMany(w => w.Bins)                 // ensure WarehouseMaster has ICollection<BinMaster> Bins
                   .HasForeignKey(x => x.WarehouseId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Bin -> Rack (required if RackId is non-nullable, optional if RackId is nullable)
            builder.HasOne(x => x.Rack)
                   .WithMany(r => r.Bins)                 // ensure RackMaster has ICollection<BinMaster> Bins; otherwise use .WithMany()
                   .HasForeignKey(x => x.RackId)
                   .OnDelete(DeleteBehavior.Restrict);

        }

    }
}