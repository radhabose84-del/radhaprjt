using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities.Power;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.Power
{
    public class GeneratorConsumptionConfiguration : IEntityTypeConfiguration<GeneratorConsumption>
    {
        public void Configure(EntityTypeBuilder<GeneratorConsumption> builder)
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

            builder.ToTable("GeneratorConsumption", "Maintenance");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
            
            builder.Property(t => t.GeneratorId)
                .HasColumnName("GeneratorId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(f => f.GeneratorTran)
                .WithMany(m => m.GeneratorConsumption)
                .HasForeignKey(f => f.GeneratorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(dg => dg.StartTime)
                .HasColumnName("StartTime")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(dg => dg.EndTime)
                .HasColumnName("EndTime")
                .HasColumnType("datetimeoffset")
                .IsRequired();

             builder.Property(t => t.RunningHours)
                .HasColumnName("RunningHours")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();
            builder.Property(t => t.DieselConsumption)
                .HasColumnName("DieselConsumption")
                .HasColumnType("decimal(10,3)") // or decimal(18,3) if you expect large values
                .IsRequired();
          
            builder.Property(t => t.OpeningEnergyReading)
                .HasColumnName("OpeningEnergyReading")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ClosingEnergyReading)
                .HasColumnName("ClosingEnergyReading")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.Energy)
                .HasColumnName("Energy")
                .HasColumnType("Decimal(18,3)")
                
                .IsRequired();

            builder.Property(ag => ag.UnitId)
               .HasColumnName("UnitId")
               .HasColumnType("int")
               .IsRequired();


            builder.Property(t => t.PurposeId)
                .HasColumnName("PurposeId")
                .HasColumnType("int");
   

            builder.HasOne(f => f.GensetPurposeType)
                .WithMany(m => m.GeneratorConsumptions)
                .HasForeignKey(f => f.PurposeId)
                .OnDelete(DeleteBehavior.Restrict);
            

            builder.Property(t => t.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

            
            builder.Property(t => t.IsDeleted)
            .HasColumnType("bit")
            .IsRequired()
            .HasConversion(isDeleteConverter);
            
            builder.Property(t => t.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(max)");

            builder.Property(t => t.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(t => t.ModifiedByName)
                .HasColumnType("varchar(max)");

            builder.Property(t => t.ModifiedIP)
                .HasColumnType("varchar(50)"); 
        }
    }
}