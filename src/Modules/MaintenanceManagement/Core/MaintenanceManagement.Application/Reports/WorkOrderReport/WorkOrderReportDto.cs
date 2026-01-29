namespace MaintenanceManagement.Application.Reports.WorkOrderReport
{
    public class WorkOrderReportDto
    {
        public DateTimeOffset? WODate { get; set; }
        public string? WorkOrderDocNo { get; set; }
        public string? CreatedUser { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Status { get; set; }
        public string? Machine { get; set; }
        public string? MachineName { get; set; }
        public int RequestId { get; set; }
        public DateTimeOffset? DowntimeStart { get; set; }
        public DateTimeOffset? DowntimeEnd { get; set; }
        public string? TotalDownTime { get; set; }
        public DateTimeOffset? MaintenanceStartTime { get; set; }
        public DateTimeOffset? MaintenanceEndTime { get; set; }
        public string? TotalMaintenanceTime { get; set; }
        public string? Department { get; set; }
        public int DepartmentId { get; set; }
        public decimal ItemCost { get; set; }
        public string? ActivityName { get; set; }
        public string? ProductionDepartment { get; set; }
        public int ProductionDepartmentId { get; set; }
        public DateTimeOffset? ClosedDate { get; set; }
        public string? ClosedUser { get; set; }
        public string? Remarks { get; set; }
        public int? ScheduleId { get; set; }
        public string? SchedulerName { get; set; }
        public int WorkOrderId { get; set; }
        
    }
}