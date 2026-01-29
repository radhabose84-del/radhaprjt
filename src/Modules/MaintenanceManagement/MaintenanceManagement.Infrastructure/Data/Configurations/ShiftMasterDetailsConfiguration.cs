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
    public class ShiftMasterDetailsConfiguration : IEntityTypeConfiguration<ShiftMasterDetail>
    {
        public void Configure(EntityTypeBuilder<ShiftMasterDetail> builder)
        {
                var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                   
                v => v ? Status.Active : Status.Inactive   
            );

                
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
            );

            builder.ToTable("ShiftMasterDetails", "Maintenance");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ShiftMasterId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(s => s.UnitId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(s => s.StartTime)
            .IsRequired()
             .HasConversion(
            v => v.ToTimeSpan(),           
            v => TimeOnly.FromTimeSpan(v)           
              );

            builder.Property(s => s.EndTime)
            .IsRequired()
            .HasConversion(
            v => v.ToTimeSpan(),
            v => TimeOnly.FromTimeSpan(v)
              );

            builder.Property(s => s.DurationInHours)
            .IsRequired()
            .HasPrecision(18, 2);

            builder.Property(s => s.BreakDurationInMinutes)
            .IsRequired()
            .HasColumnType("int");

             builder.Property(s => s.EffectiveDate)
            .IsRequired()
            .HasColumnType("date");  

            builder.Property(s => s.ShiftSupervisorId)
            .IsRequired()
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

                 builder.HasOne(s => s.ShiftMaster)
                .WithMany(c => c.ShiftMasterDetails)
                .HasForeignKey(s => s.ShiftMasterId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}