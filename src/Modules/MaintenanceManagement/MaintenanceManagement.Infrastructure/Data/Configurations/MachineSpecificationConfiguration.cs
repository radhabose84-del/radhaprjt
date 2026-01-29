using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class MachineSpecificationConfiguration : IEntityTypeConfiguration<MachineSpecification>
    {
        public void Configure(EntityTypeBuilder<MachineSpecification> builder)
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
            builder.ToTable("MachineSpecification", "Maintenance");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(s => s.SpecificationId)
                .IsRequired()
                .HasColumnType("int");

            // Foreign Key configuration: required one-to-many
            builder.HasOne(m => m.SpecificationIdMachineSpec)
                   .WithMany(c => c.MachineSpecificationsName)
                   .HasForeignKey(m => m.SpecificationId)
                   .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);


            builder.Property(s => s.SpecificationValue)
                .HasColumnType("nvarchar(100)")
                .IsRequired(); 
    
 
            builder.Property(s => s.MachineId)
            .IsRequired()
             .HasColumnType("int");

            // Foreign Key configuration: required one-to-many
            builder.HasOne(m => m.MachineMaster)
            .WithMany(c => c.MachineSpecification)
            .HasForeignKey(m => m.MachineId)
             .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
      
        }
    }
}