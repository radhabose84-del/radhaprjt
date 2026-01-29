using MaintenanceManagement.Application.Common.Mappings;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class WorkOrderUpdateDto :  IMapFrom<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>
    {        
        public int Id { get; set; }
        public int CompanyId { get; set; } 
        public int UnitId { get; set; } 
        public string? WorkOrderDocNo { get; set; }
        public int? RequestId { get; set; }                
        public int? PreventiveScheduleId { get; set; }         
        public int? StatusId { get; set; }     
        public int? RootCauseId { get; set; }         
        public string? Remarks { get; set; }
        public string? Image { get; set; }           
        public int? TotalManPower { get; set; }         
        public decimal? TotalSpentHours { get; set; } 
        public DateTimeOffset? DownTimeStart { get; set; } 
        public DateTimeOffset? DownTimeEnd { get; set; }          
        
        
        public ICollection<WorkOrderActivityUpdateDto>? WorkOrderActivity{ get; set; }               
        public ICollection<WorkOrderTechnicianUpdateDto>? WorkOrderTechnician{ get; set; } 
        public ICollection<WorkOrderItemUpdateDto>? WorkOrderItem{ get; set; } 
        public ICollection<WorkOrderCheckListUpdateDto>? WorkOrderCheckList{ get; set; }
    }
}