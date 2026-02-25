using WarehouseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.Infrastructure.Data.Configurations
{
    public class RackMasterConfiguration : IEntityTypeConfiguration<RackMaster>
    {
        public void Configure(EntityTypeBuilder<RackMaster> builder)
        {
                 var statusConverter = new ValueConverter<Status, bool>(
                    v => v == Status.Active,                    // Convert to DB (1 for Active)
                    v => v ? Status.Active : Status.Inactive    // Convert to Entity
                );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
               );
            

            builder.ToTable("RackMaster", "Warehouse");
            builder.HasKey(x => x.Id);

              builder.Property(x => x.WarehouseId)
                   .IsRequired();

            builder.Property(x => x.RackCode)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnType("nvarchar(50)");

            builder.Property(x => x.RackName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.FloorId)
                   .IsRequired()
                   .HasColumnType("int");

            builder.Property(x => x.AisleId)
                    .HasColumnType("int")
                    .IsRequired();

            builder.Property(x => x.RackLevelId)
                    .HasColumnType("int")
                    .IsRequired(); 
         
              builder.Property(x => x.MaxCapacity)
                   .HasPrecision(18, 2);

            builder.Property(x => x.CapacityUOMId)
                   .HasColumnType("int");

            builder.Property(x => x.RackWidth)
                   .HasPrecision(18, 2);

            builder.Property(x => x.RackHeight)
                   .HasPrecision(18, 2);

            builder.Property(x => x.DimensionUOMId)
                   .HasColumnType("int");


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


            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(255)");

           

            // Rack -> Warehouse (many-to-one)
            builder.HasOne(x => x.Warehouse)
            .WithMany(w => w.Racks)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict); // avoid cascading deletes across core masters

            // Optional unique per warehouse
            builder.HasIndex(x => new { x.WarehouseId, x.RackCode })
            .IsUnique()
            .HasDatabaseName("UX_Rack_Warehouse_RackCode");

        }
        
    }
}