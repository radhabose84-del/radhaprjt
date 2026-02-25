namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate
{
    public class SchedulerByDateDto
    {
        public int TotalScheduleCount { get; set; }
        public string ScheduleDate { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
    }
}