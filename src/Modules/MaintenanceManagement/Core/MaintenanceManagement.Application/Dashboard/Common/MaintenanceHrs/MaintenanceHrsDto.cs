namespace MaintenanceManagement.Application.Dashboard.MaintenanceHrs
{
    public class MaintenanceHrsDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal MaintenanceHrs { get; set; }      
        public decimal DowntimeHrs  { get; set; }      
    }
}
