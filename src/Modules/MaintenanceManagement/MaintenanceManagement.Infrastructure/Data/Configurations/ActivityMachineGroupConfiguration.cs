using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class ActivityMachineGroupConfiguration  : IEntityTypeConfiguration<ActivityMachineGroup>
    {
       
       public void Configure(EntityTypeBuilder<ActivityMachineGroup> builder)
        { 
            
          builder.ToTable("ActivityMachineGroup", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ActivityMasterId)
                .HasColumnName("ActivityMasterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MachineGroupId)
                .HasColumnName("MachineGroupId")
                .HasColumnType("int")
                .IsRequired();

                 // Foreign Key: ActivityMaster
            builder.HasOne(amg => amg.ActivityMaster)
                .WithMany(am => am.ActivityMachineGroups)
                .HasForeignKey(amg => amg.ActivityMasterId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Choose Restrict or Cascade

            // Foreign Key: MachineGroup
            builder.HasOne(amg => amg.MachineGroup)
                .WithMany(mg => mg.ActivityMachineGroups)
                .HasForeignKey(amg => amg.MachineGroupId)
                .OnDelete(DeleteBehavior.Cascade);
         


        }

        
    }
}