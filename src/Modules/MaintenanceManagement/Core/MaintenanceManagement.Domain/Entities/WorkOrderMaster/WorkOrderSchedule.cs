using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.WorkOrderMaster
{
    public class WorkOrderSchedule 
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder WOSchedule { get; set; } = null!; 
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }   
        public byte? IsCompleted { get; set; }   =0;
        public int StatusId { get; set; }           
        public required MiscMaster MiscStatus { get; set; }  
    }
}