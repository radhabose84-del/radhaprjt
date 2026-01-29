
namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById
{
    public class GetWorkOrderByIdDto
    {   
        
        public string? WorkOrderDocNo { get; set; }                   
        public string? Remarks { get; set; }
        public string? Image { get; set; }
        public string? ImagePath { get; set; }
        public int StatusId { get; set; }                         
        public string? StatusDesc { get; set; }
        public int RootCauseId { get; set; }  
        public string? RootCauseDesc { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? DownTimeStart { get; set; }
        public DateTimeOffset? DownTimeEnd { get; set; }
        public string? Machine { get; set; }
        public string? MachineName { get; set; }
        public string? Department { get; set; }
        public int DepartmentId { get; set; }
        public int? RequestId { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetLocation { get; set; }        
        public string? RequestDesc { get; set; }
        public int? PreventiveScheduleId { get; set; }
        public string? CreatedUser { get; set; }
        public string? RequestRemarks { get; set; }
        public string? RequestBy { get; set; }

        public IList<GetWorkOrderActivityByIdDto>? WOActivity { get; set; }
        public IList<GetWorkOrderItemByIdDto>? WOItem { get; set; }        
        public IList<GetWorkOrderTechnicianByIdDto>? WOTechnician { get; set; }
        public IList<GetWorkOrderCheckListByIdDto>? WOCheckList { get; set; }
        public IList<GetWorkOrderScheduleByIdDto>? WOSchedule { get; set; }
    } 
}