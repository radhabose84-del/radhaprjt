
namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
{
    public class GetWorkOrderScheduleByIdDto
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }      
        public byte IsCompleted { get; set; }
        public string? ScheduleStatus { get; set; }         
    }
}