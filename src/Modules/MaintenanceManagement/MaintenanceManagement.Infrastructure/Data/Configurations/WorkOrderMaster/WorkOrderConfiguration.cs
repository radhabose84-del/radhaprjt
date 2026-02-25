using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster
{
    public class WorkOrderConfiguration  : IEntityTypeConfiguration<WorkOrder>
    {       
       public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {           
            
            builder.ToTable("WorkOrder", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();  
            
            builder.Property(t => t.WorkOrderDocNo)
                .HasColumnName("WorkOrderDocNo")
                .HasColumnType("varchar(50)")
                .IsRequired();             

            builder.Property(t => t.RequestId)
                .HasColumnName("RequestId")
                .HasColumnType("int");               
            builder.HasOne(amg => amg.WOMaintenanceRequest)
                .WithMany(mg => mg.WorkOrdersRequest)
                .HasForeignKey(amg => amg.RequestId)
                 .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(t => t.PreventiveScheduleId)
                .HasColumnName("PreventiveScheduleId")
                .HasColumnType("int");                
            builder.HasOne(amg => amg.WOPreventiveScheduler)
                .WithMany(mg => mg.workOrdersSchedule)
                .HasForeignKey(amg => amg.PreventiveScheduleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);           

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();           
            builder.HasOne(amg => amg.MiscStatus)
                .WithMany(mg => mg.WorkOrderStatus)
                .HasForeignKey(amg => amg.StatusId)
                .OnDelete(DeleteBehavior.Restrict);         
            
            builder.Property(t => t.RootCauseId)
                .HasColumnName("RootCauseId")
                .HasColumnType("int")
                .IsRequired(false);     
            builder.HasOne(amg => amg.MiscRootCause)
                .WithMany(am => am.WorkOrderRootCause)
                .HasForeignKey(amg => amg.RootCauseId)
                .OnDelete(DeleteBehavior.Restrict);    
            
            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(1000)")
                .IsRequired(false);     

            builder.Property(t => t.Image)
                .HasColumnName("Image")
                .HasColumnType("varchar(250)")
                .IsRequired(false);
            
            builder.Property(t => t.TotalManPower)
                .HasColumnName("TotalManPower")
                .HasColumnType("int")
                .IsRequired(false);   
            
            builder.Property(t => t.TotalSpentHours)
                .HasColumnName("TotalSpentHours")
                .HasColumnType("decimal(5,2)")
                .IsRequired(false);
            
            builder.Property(t => t.DowntimeStart)
                .HasColumnName("DowntimeStart")                
                .HasColumnType("DateTimeOffset")
                .IsRequired(false);   
            
            builder.Property(t => t.DowntimeEnd)
                .HasColumnName("DowntimeEnd")                
                .HasColumnType("DateTimeOffset")
                .IsRequired(false);              
          
            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");
                    
            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(100)"); 
        }
    }
}