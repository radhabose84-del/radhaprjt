namespace MaintenanceManagement.Application.Reports.ScheduleReport
{
    public class ScheduleReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public string MachineName { get; set; } = default!;
        public string GroupName { get; set; } = default!;
        public string MaintenanceCategory { get; set; } = default!;
        public string ActivityName { get; set; } = default!;
        public string ActivityType { get; set; } = default!;
        public string DueDate { get; set; } = default!;
        public string LastCompletionDate { get; set; } = default!;
        public string PreventiveSchedulerName { get; set; } = default!;
        public string MachineCode { get; set; } = default!;
        public string WorkOrderStatus { get; set; } = default!;
        public string WorkOrderDocNo { get; set; } = default!;
        public int ProductionDepartmentId { get; set; }
        public string ProductionDepartmentName { get; set; } = default!;
        public string PendingDays { get; set; } = default!;
        public int UnitId { get; set; }
    }
}