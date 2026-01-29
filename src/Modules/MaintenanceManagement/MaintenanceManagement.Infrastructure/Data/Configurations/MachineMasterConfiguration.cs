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
    public class MachineMasterConfiguration : IEntityTypeConfiguration<MachineMaster>
    {
        public void Configure(EntityTypeBuilder<MachineMaster> builder)
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
            builder.ToTable("MachineMaster", "Maintenance");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ag => ag.MachineCode)
                .HasColumnName("MachineCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(ag => ag.MachineName)
                .HasColumnName("MachineName")
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            builder.Property(s => s.MachineGroupId)
                .IsRequired()
                .HasColumnType("int");

            // Foreign Key: MachineGroup (One-to-Many)
            builder.HasOne(m => m.MachineGroup)
               .WithMany(g => g.MachineMasters)  // Assuming Machines is a ICollection<MachineMaster> in MachineGroup
               .HasForeignKey(m => m.MachineGroupId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if required


            builder.Property(ag => ag.UnitId)
               .HasColumnName("UnitId")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(dg => dg.ProductionCapacity)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false)
                .HasDefaultValue(0.00m); // Set default value to 0.00

            builder.Property(ag => ag.UomId)
                .HasColumnName("UomId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(s => s.ShiftMasterId)
                .IsRequired()
                .HasColumnType("int");

            // Foreign Key: ShiftMaster (One-to-Many)
            builder.HasOne(m => m.ShiftMaster)
               .WithMany(s => s.MachineMasters)
               .HasForeignKey(m => m.ShiftMasterId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.CostCenterId)
                .IsRequired()
                .HasColumnType("int");

            // Foreign Key: CostCenter (One-to-Many)
            builder.HasOne(m => m.CostCenter)
               .WithMany(c => c.MachineMasters)
               .HasForeignKey(m => m.CostCenterId)
               .OnDelete(DeleteBehavior.Restrict);


            builder.Property(s => s.WorkCenterId)
                .IsRequired()
                .HasColumnType("int");

            // Foreign Key: WorkCenter (One-to-Many)
            builder.HasOne(m => m.WorkCenter)
               .WithMany(w => w.MachineMasters)
               .HasForeignKey(m => m.WorkCenterId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(dg => dg.InstallationDate)
                .HasColumnName("InstallationDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(s => s.AssetId)
                .IsRequired()
                .HasColumnType("int");


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

            builder.Property(s => s.LineNo)
             .IsRequired()
              .HasColumnType("int");

            // Foreign Key configuration: required one-to-many
            builder.HasOne(m => m.LineNoMachine)
            .WithMany(c => c.MachineMasterLineNo)
            .HasForeignKey(m => m.LineNo)
             .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.IsProductionMachine)
                .HasColumnName("IsProductionMachine")
                .HasColumnType("bit")
                .IsRequired();

        }
    }
}