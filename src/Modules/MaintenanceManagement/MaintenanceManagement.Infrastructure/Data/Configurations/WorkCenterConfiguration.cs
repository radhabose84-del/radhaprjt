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
    public class WorkCenterConfiguration : IEntityTypeConfiguration<WorkCenter>
    {
         public void Configure(EntityTypeBuilder<WorkCenter> builder)
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
            builder.ToTable("WorkCenter", "Maintenance");
                // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.WorkCenterCode)
                .HasColumnName("WorkCenterCode")
                .HasColumnType("varchar(10)")
                .IsRequired();  

            builder.Property(ag => ag.WorkCenterName)
                .HasColumnName("WorkCenterName")
                .HasColumnType("varchar(100)")
                .IsRequired(); 

             builder.Property(ag => ag.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired(); 

             builder.Property(ag => ag.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired(); 
           
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