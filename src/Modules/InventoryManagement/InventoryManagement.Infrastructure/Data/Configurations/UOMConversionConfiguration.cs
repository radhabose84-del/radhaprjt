using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class UOMConversionConfiguration : IEntityTypeConfiguration<UOMConversion>
    {
        public void Configure(EntityTypeBuilder<UOMConversion> builder)
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

            builder.ToTable("UOMConversion", "Inventory");

            // Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromUOMId)
                  .IsRequired();

            builder.Property(x => x.ToUOMId)
                   .IsRequired();

            builder.Property(x => x.ConversionValue)
                   .HasColumnType("decimal(18,6)")
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


            // Relationship for FromUOM
            builder.HasOne(uc => uc.FromUOM)
                   .WithMany(u => u.FromUOMConversions)
                   .HasForeignKey(uc => uc.FromUOMId)
                   .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

            //  Relationship for ToUOM
            builder.HasOne(uc => uc.ToUOM)
                   .WithMany(u => u.ToUOMConversions)
                   .HasForeignKey(uc => uc.ToUOMId)
                   .OnDelete(DeleteBehavior.Restrict);
                   

          


        }
        
    }
}