
namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder
{
    public class ScheduleDto
    {
        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public byte IsCompleted { get; set; } 
    }
    public class GetWorkOrderDto
    {
        public int Id { get; set; }                   
        public string? WorkOrderDocNo { get; set; }
        public string? Department { get; set; }
        public int DepartmentId { get; set; }        
        public string? Machine { get; set; }
        public DateTimeOffset? RequestDate { get; set; }
        public string? RequestType { get; set; }
        public string? Status { get; set; }   
        public string? MaintenanceType { get; set; }  
        public int RequestId { get; set; }
        public string? MachineName { get; set; }
        public string? ScheduleStatus { get; set; }
        public string? DueDate { get; set; }
        public string? ActivityName { get; set; }
        public string? RequestRemarks { get; set; }
        public string? RequestBy { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
    }
}