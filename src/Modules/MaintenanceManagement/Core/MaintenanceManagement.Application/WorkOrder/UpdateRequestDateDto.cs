
namespace MaintenanceManagement.Application.WorkOrder
{
    public class UpdateRequestDateDto
    {
        public int workOrderId { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public int IsSystemTime { get; set; }  
    }
}
