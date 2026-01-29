
namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
{
    public class GetWorkOrderActivityByIdDto
    {                       
        public int ActivityId { get; set; }        
        public string? ActivityName { get; set;}
        public string? Description { get; set; }                
    }
}