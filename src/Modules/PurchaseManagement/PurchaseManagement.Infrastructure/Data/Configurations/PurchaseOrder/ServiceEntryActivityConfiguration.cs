using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class ServiceEntryActivityConfiguration : IEntityTypeConfiguration<ServiceEntryActivity>
    {
        public void Configure(EntityTypeBuilder<ServiceEntryActivity> b)
        {
           // Table configuration
            b.ToTable("ServiceEntryActivities", "Purchase");

            // Primary key configuration
            b.HasKey(x => x.Id);          

            b.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false); // Optional field

            b.Property(x => x.StatusRemarks)
            .HasMaxLength(500)
            .IsRequired(false);

            // Foreign key relationships configuration
            b.HasOne(a => a.EntrySheet)
            .WithMany(s => s.Activities) // Reverse navigation property
            .HasForeignKey(a => a.EntrySheetId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete to maintain integrity

           b.HasOne(a => a.ActivityType)
            .WithMany(m => m.ActivityTypes) // Reverse navigation from MiscMaster to ServiceEntryActivity
            .HasForeignKey(a => a.ActivityTypeId)
           .OnDelete(DeleteBehavior.NoAction); // Set null if ActivityType is deleted

           b.HasOne(a => a.SESActivityStatus)
            .WithMany(m => m.SESActivityStatuses) // Reverse navigation from MiscMaster to ServiceEntryActivity
            .HasForeignKey(a => a.SESActivityStatusId)
           .OnDelete(DeleteBehavior.NoAction); // Set null if SESActivityStatus is deleted

            // Indexes to speed up searches
            b.HasIndex(x => x.EntrySheetId);
            b.HasIndex(x => x.SESActivityStatusId);
           
        }
    }
}