using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static MaintenanceManagement.Domain.Common.BaseEntity;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class MachineGroupConfiguration :IEntityTypeConfiguration<MachineGroup>
    {

        public void Configure(EntityTypeBuilder<MachineGroup> builder)
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

            builder.ToTable("MachineGroup", "Maintenance");

            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(mg => mg.GroupName)
                .HasColumnName("GroupName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(mg => mg.Manufacturer)
            .HasColumnName("Manufacturer")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(mg => mg.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(mg => mg.DepartmentId)
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
                
              builder.Property(t => t.PowerSource)
                .HasColumnName("PowerSource")
                .HasColumnType("bit")
                .IsRequired();

        }
    }
}