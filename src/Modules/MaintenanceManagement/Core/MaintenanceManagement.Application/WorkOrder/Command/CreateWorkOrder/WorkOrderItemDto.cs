
namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
{
    public class WorkOrderItemDto
    {
        public int? StoreTypeId { get; set; }     
        public string? ItemCode { get; set; }   
        public string? OldItemCode { get; set; }           
        public string? ItemName { get; set; }        
        public int AvailableQty { get; set; }
        public int UsedQty { get; set; }
        public int? ScarpQty { get; set; }
        public int? ToSubStoreQty { get; set; }
        public string? Image { get; set; }
        public decimal? Rate { get; set; }
    }
}