using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class PurchaseOrderServiceScheduleConfiguration : IEntityTypeConfiguration<PurchaseOrderServiceSchedule>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderServiceSchedule> b)
        {
             // Define the table name and schema
            b.ToTable("PurchaseOrderServiceSchedule", "Purchase");

            // Primary Key (Id inherited from BaseEntity)
            b.HasKey(x => x.Id);

            // Required Foreign Keys
            b.Property(x => x.PurchaseOrderId).IsRequired();  
            b.Property(x => x.ServicePoHeaderId).IsRequired();
            b.Property(x => x.ServiceItemId).IsRequired();    
            b.Property(x => x.ScheduleNo).IsRequired();       

            // Optional fields
            b.Property(x => x.OccurrencePeriod).HasMaxLength(100).IsUnicode(true);  
            b.Property(x => x.OccurrenceDescription).HasMaxLength(500).IsUnicode(true); 
            b.Property(x => x.ActivityTypeId); 
            b.Property(x => x.PlannedDurationHrs).HasPrecision(9, 2); 
            b.Property(x => x.DueDate);  
            b.Property(x => x.ServiceStartDate); 
            b.Property(x => x.ServiceEndDate);  
            b.Property(x => x.PlannedQuantity).HasPrecision(18, 3);  
            b.Property(x => x.PlannedRate).HasPrecision(18, 2);  
            b.Property(x => x.PlannedValue).HasPrecision(18, 2);  
            b.Property(x => x.Remarks).HasMaxLength(500).IsUnicode(true);             
            b.Property(x => x.AutoGenerateSES).IsRequired();  

            b.HasOne(x => x.ServicePoHeader)        // use the nav, not the type
             .WithMany()
             .HasForeignKey(x => x.ServicePoHeaderId)
             .OnDelete(DeleteBehavior.NoAction);

            // schedule → line (CASCADE is OK)
            b.HasOne<PurchaseOrderServiceLine>()
             .WithMany(l => l.PurchaseOrderServiceSchedules)
             .HasForeignKey(x => x.ServiceItemId)
             .OnDelete(DeleteBehavior.Cascade);
            

        }
    }
}