namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class WorkOrderActivityUpdateDto
    {
        public int WorkOrderId { get; set; }
        public int ActivityId { get; set; }        
        public string? Description { get; set; }       
    }
}