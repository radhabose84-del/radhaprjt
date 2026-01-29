using MaintenanceManagement.Application.Common.Mappings;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
{
    public class WorkOrderCombineDto :  IMapFrom<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>
    {            
        public int? RequestId { get; set; }                
        public int? PreventiveScheduleId { get; set; }         
        public int StatusId { get; set; }                      
        public string? Remarks { get; set; }        
        public int RequestTypeId { get; set; } 
        public string? Image { get; set; } 
        public DateTimeOffset? DownTimeStart { get; set; } 
        public DateTimeOffset? DownTimeEnd { get; set; }          

        
        public ICollection<WorkOrderActivityDto>? WorkOrderActivity{ get; set; }                          
        public ICollection<WorkOrderItemDto>? WorkOrderItem{ get; set; } 
        public ICollection<WorkOrderCheckListDto>? WorkOrderCheckList{ get; set; } 
    }
}