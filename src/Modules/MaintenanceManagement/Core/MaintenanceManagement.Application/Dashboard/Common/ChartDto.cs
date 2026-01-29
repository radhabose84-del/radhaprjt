namespace MaintenanceManagement.Application.Dashboard.Common
{
    public class ChartDto
    {        
        public List<string>? Categories { get; set; }
        public List<ChartSeriesDto>? Series { get; set; }
    }
}