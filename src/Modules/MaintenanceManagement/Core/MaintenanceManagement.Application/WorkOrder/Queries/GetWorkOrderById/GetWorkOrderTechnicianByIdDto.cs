
namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
{
    public class GetWorkOrderTechnicianByIdDto
    {
        public int? CustodianId { get; set; }
        public int? OldCustodianId { get; set; }
        public string? CustodianName { get; set; }
        public int? SourceId { get; set; }   
        public string? SourceDesc { get; set; }   
        public int HoursSpent { get; set; }    
        public int MinutesSpent { get; set; } 
    }

}