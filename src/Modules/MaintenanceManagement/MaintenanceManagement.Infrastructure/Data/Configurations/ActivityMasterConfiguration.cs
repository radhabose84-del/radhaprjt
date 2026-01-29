using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class ActivityMasterConfiguration : IEntityTypeConfiguration<ActivityMaster>
    {
        public void Configure(EntityTypeBuilder<ActivityMaster> builder)
        {
         var statusConverter = new ValueConverter<Status, bool>(
                    v => v == Status.Active,                    
                    v => v ? Status.Active : Status.Inactive    
                );
          
                var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                    v => v == IsDelete.Deleted,                 
                    v => v ? IsDelete.Deleted : IsDelete.NotDeleted
                );
            builder.ToTable("ActivityMaster", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
            
            builder.Property(t => t.ActivityName)
                .HasColumnName("ActivityName")
                .HasColumnType("varchar(250)")
                .IsRequired();
            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(500)")
                .IsRequired();

                 builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();
                
                builder.Property(t => t.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();          

            builder.Property(t => t.EstimatedDuration)
                .HasColumnName("EstimatedDuration")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ActivityType)
                .HasColumnName("ActivityType")
                .HasColumnType("int")
                .IsRequired();
            
                // MachineGroup → ActivityMaster (One-to-Many Relationship)
            builder.HasOne(am => am.ActivityTypes)  // One MachineGroup per ActivityMaster
            .WithMany(mg => mg.ActivityType) // One MachineGroup has many ActivityMasters
            .HasForeignKey(am => am.ActivityType) // Foreign key in ActivityMaster
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete   


               
        }
    }
}