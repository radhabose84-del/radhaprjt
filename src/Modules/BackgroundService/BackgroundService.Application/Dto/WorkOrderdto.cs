using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Dto
{
    public class WorkOrderdto
     {
        public int CompanyId { get; set; } 
        public int UnitId { get; set; }         
        public string? WorkOrderDocNo { get; set; }
        public int? RequestId { get; set; }
        public int? PreventiveScheduleId { get; set; }       
        public int StatusId { get; set; }       
        public int? RootCauseId { get; set; }                  
        public string? Remarks { get; set; }
        public string? Image { get; set; }
        public int? TotalManPower { get; set; }
        public decimal? TotalSpentHours { get; set; }       
        public DateTimeOffset? DowntimeStart { get; set; }  
        public DateTimeOffset? DowntimeEnd { get; set; }           
        public ICollection<WorkOrderItemDto>? WorkOrderItems  {get; set;}  
        public ICollection<WorkOrderActivityDto>? WorkOrderActivities  {get; set;}  
    }
}