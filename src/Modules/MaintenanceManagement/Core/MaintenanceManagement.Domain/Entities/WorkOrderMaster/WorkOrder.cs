using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.WorkOrderMaster    
{
    public class WorkOrder :CommonEntity
    {
        public int CompanyId { get; set; } 
        public int UnitId { get; set; }         
        public string? WorkOrderDocNo { get; set; }
        public int? RequestId { get; set; }
        public MaintenanceRequest? WOMaintenanceRequest { get; set; } 
        public int? PreventiveScheduleId { get; set; }                          
        public PreventiveSchedulerDetail? WOPreventiveScheduler { get; set; }
        public int StatusId { get; set; } 
        public required MiscMaster MiscStatus { get; set; }       
        public int? RootCauseId { get; set; }          
        public MiscMaster MiscRootCause { get; set; } = default!;
        public string? Remarks { get; set; }
        public string? Image { get; set; }
        public int? TotalManPower { get; set; }
        public decimal? TotalSpentHours { get; set; }       
        public DateTimeOffset? DowntimeStart { get; set; }  
        public DateTimeOffset? DowntimeEnd { get; set; }           
        public ICollection<WorkOrderItem>? WorkOrderItems  {get; set;}  
        public ICollection<WorkOrderActivity>? WorkOrderActivities  {get; set;}  
        public ICollection<WorkOrderSchedule>? WorkOrderSchedules  {get; set;}  
        public ICollection<WorkOrderTechnician>? WorkOrderTechnicians {get; set;} 
        public ICollection<WorkOrderCheckList>? WorkOrderCheckLists {get; set;} 

         
        
       

    }
}