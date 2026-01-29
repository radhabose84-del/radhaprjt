namespace MaintenanceManagement.Application.Dashboard.CardView
{
    public class CardViewDto
    {
        public int? TotalSchedules { get; set; }
        public decimal? MaintenanceHrs { get; set; }
        public decimal? DowntimeHrs { get; set; }
        public decimal? ConsumptionValue { get; set; }
        public int? OpenWorkOrder { get; set; }
        public int? InProgressWorkOrder { get; set; }
        public int? ClosedWorkOrder { get; set; }
        public int? OverDueWorkOrder { get; set; }
        public List<ConsumptionDto> TopConsumptions { get; set; } = new();
    }
}
