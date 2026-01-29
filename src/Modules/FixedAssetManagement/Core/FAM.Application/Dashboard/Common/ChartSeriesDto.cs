namespace FAM.Application.Dashboard.Common
{
    public class ChartSeriesDto
    {
        public string? Name { get; set; } 
        public List<decimal>? Data { get; set; } = new();
    }
}