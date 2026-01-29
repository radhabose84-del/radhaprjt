using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class MaintenanceCategoryConfiguration : IEntityTypeConfiguration<MaintenanceCategory>
    {
        public void Configure(EntityTypeBuilder<MaintenanceCategory> builder)
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
            builder.ToTable("MaintenanceCategory", "Maintenance");
                // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.CategoryName)
                .HasColumnName("CategoryName")
                .HasColumnType("varchar(100)")
                .IsRequired();  

            builder.Property(ag => ag.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)");
               
           
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