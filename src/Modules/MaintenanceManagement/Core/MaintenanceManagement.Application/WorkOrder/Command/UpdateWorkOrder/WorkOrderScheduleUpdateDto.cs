

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class WorkOrderScheduleUpdateDto
    {
        public int? WorkOrderId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public byte? IsCompleted { get; set; }
        public int StatusId { get; set; }  
        public int IsSystemTime { get; set; }  
    }
}