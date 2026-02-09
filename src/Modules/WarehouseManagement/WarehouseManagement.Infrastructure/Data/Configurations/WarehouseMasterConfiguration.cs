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
    public class WarehouseMasterConfiguration : IEntityTypeConfiguration<WarehouseMaster>
    {

        public void Configure(EntityTypeBuilder<WarehouseMaster> builder)
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


            builder.ToTable("WarehouseMaster", "Warehouse");
            // Primary Key
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(w => w.WarehouseCode)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.HasIndex(w => w.WarehouseCode).IsUnique();

            builder.Property(w => w.WarehouseName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(w => w.UnitId)
                   .IsRequired();

            builder.Property(w => w.ParentWarehouseId);

            builder.Property(w => w.IsGroup)
                   .IsRequired();

            builder.Property(w => w.WarehouseTypeId)
                   .IsRequired();

            builder.Property(w => w.DepartmentId)
                   .IsRequired();     

            builder.Property(w => w.StorageTypeId)
                   .IsRequired();

            builder.Property(w => w.CapacityUOMId)
                   .IsRequired();

            builder.Property(w => w.AccountId);

            builder.Property(w => w.ContactPersonName)
                   .HasMaxLength(100);

            builder.Property(w => w.MobileNumber)
                   .HasMaxLength(20);

            builder.Property(w => w.Email)
                   .HasMaxLength(100);

            builder.Property(w => w.AddressLine1)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(w => w.AddressLine2)
                   .HasMaxLength(200);

            builder.Property(w => w.CityId)
                   .IsRequired();

            builder.Property(w => w.StateId)
                   .IsRequired();

            builder.Property(w => w.CountryId)
                   .IsRequired();

            builder.Property(w => w.Pincode)
                   .HasMaxLength(10);

            builder.Property(w => w.IsScrapWarehouse)
                   .IsRequired();

            builder.Property(w => w.IsTransitWarehouse)
                   .IsRequired();

            builder.Property(w => w.MaxCapacity)
                   .HasColumnType("decimal(18,2)");

            builder.Property(w => w.IsDefaultStockEntry)
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
       

            // Relationships
            builder.HasOne(w => w.ParentWarehouse)
                   .WithMany(p => p.ChildWarehouses)
                   .HasForeignKey(w => w.ParentWarehouseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.AllowedItemGroups)
                   .WithOne(m => m.Warehouse)
                   .HasForeignKey(m => m.WarehouseId)
                   .OnDelete(DeleteBehavior.Cascade);
                   


        }
    }
}