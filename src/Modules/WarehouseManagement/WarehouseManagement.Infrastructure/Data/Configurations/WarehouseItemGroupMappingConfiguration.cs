using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.Infrastructure.Data.Configurations
{
    public class WarehouseItemGroupMappingConfiguration : IEntityTypeConfiguration<WarehouseItemGroupMapping>
    {
        public void Configure(EntityTypeBuilder<WarehouseItemGroupMapping> builder)
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
            
            builder.HasKey(x => x.Id);

            builder.ToTable("WarehouseItemGroupMapping", "Warehouse");

             builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                   .HasColumnName("Id")
                   .HasColumnType("int")
                   .IsRequired();

            // FK to WarehouseMaster
            builder.Property(m => m.WarehouseId)
                   .IsRequired()
                   .HasColumnType("int");

            // Soft FK to ItemGroup (Inventory Service)
            builder.Property(m => m.ItemGroupId)
                   .IsRequired()
                   .HasColumnType("int");

            // Unique constraint to avoid duplicate mappings
            builder.HasIndex(m => new { m.WarehouseId, m.ItemGroupId })
                   .IsUnique();

            // BaseEntity properties
           builder.Property(m => m.IsActive)
                    .HasColumnName("IsActive")
                    .HasColumnType("bit")
                    .HasConversion(statusConverter)
                    .IsRequired();

            builder.Property(m => m.IsDeleted)
                    .HasColumnName("IsDeleted")
                    .HasColumnType("bit")
                    .HasConversion(isDeleteConverter)
                    .IsRequired();

            builder.Property(m => m.CreatedByName)
                   .IsRequired()
                   .HasColumnType("varchar(50)");

            builder.Property(m => m.CreatedIP)
                   .IsRequired()
                   .HasColumnType("varchar(255)");

            builder.Property(m => m.ModifiedByName)
                   .HasColumnType("varchar(50)");

            builder.Property(m => m.ModifiedIP)
                   .HasColumnType("varchar(255)");

            // Relationship with WarehouseMaster
            builder.HasOne(m => m.Warehouse)
                   .WithMany(w => w.AllowedItemGroups)
                   .HasForeignKey(m => m.WarehouseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}