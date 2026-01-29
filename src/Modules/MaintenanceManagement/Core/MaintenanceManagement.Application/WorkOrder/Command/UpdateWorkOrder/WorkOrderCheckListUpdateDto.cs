
namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class WorkOrderCheckListUpdateDto
    {
        public int? WorkOrderId { get; set; }
        public int CheckListId { get; set; }   
        public byte IsCompleted { get; set; }    
        public string? Description { get; set; }   
    }
}