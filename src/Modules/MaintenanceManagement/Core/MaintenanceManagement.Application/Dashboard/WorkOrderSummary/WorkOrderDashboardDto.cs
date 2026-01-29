namespace MaintenanceManagement.Application.Dashboard.WorkOrderSummary
{
    public class WorkOrderDashboardDto
    {
        public string? Month { get; set; }
        public string? StatusName { get; set; }
        public decimal Total { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}