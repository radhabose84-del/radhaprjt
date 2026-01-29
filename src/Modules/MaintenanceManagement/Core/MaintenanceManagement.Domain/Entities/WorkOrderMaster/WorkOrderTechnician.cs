 using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.WorkOrderMaster
{
    public class WorkOrderTechnician 
    {
        public int? Id { get; set; }
        public int? WorkOrderId { get; set; }
        public WorkOrder WOTechnician { get; set; } = null!; 
        public int TechnicianId { get; set; }
        public int OldTechnicianId { get; set; }        
        public int SourceId { get; set; }
        public MiscMaster MiscSource { get; set; } = null!;   
        public string? TechnicianName { get; set; }
        public int HoursSpent { get; set; }        
        public int MinutesSpent { get; set; }    
    }
}