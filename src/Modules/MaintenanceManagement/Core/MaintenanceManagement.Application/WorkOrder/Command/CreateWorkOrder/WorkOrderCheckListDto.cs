
namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
{
    public class WorkOrderCheckListDto
    {        
        public int CheckListId { get; set; }  
        public byte ISCompleted { get; set; }     
        public string? Description { get; set; }                
    }
}