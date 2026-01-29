using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class ActivityCheckListMasterConfiguration   : IEntityTypeConfiguration<ActivityCheckListMaster>
    {

         public void Configure(EntityTypeBuilder<ActivityCheckListMaster> builder)
        { 

           builder.ToTable("ActivityCheckListMaster", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();


                builder.Property(t => t.ActivityId)
                .HasColumnName("ActivityID")
                .HasColumnType("int")
                .IsRequired();
              
                builder.Property(t => t.ActivityCheckList)
                .HasColumnName("ActivityCheckList")
                .HasColumnType("varchar(250)")
                .IsRequired();

                builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

               

                builder.HasOne(ac => ac.ActivityMaster)  
                .WithMany(am => am.ActivityCheckLists)  // Ensure property name in ActivityMaster
                .HasForeignKey(ac => ac.ActivityId)  // ✅ Correct Foreign Key Reference
                .OnDelete(DeleteBehavior.Cascade);  
               
        }

    }
}