namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById
{
    public class GetClosingReaderValueDto
    {
        public int FeederId { get; set; }
        public string? FeederCode { get; set; }
        public string? FeederName { get; set; }
        public decimal OpeningReading { get; set; }
    }
}