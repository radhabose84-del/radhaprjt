using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MaintenanceRequestReport
{
    public class RequestReportQuery : IRequest<ApiResponseDTO<List<RequestReportDto>>> 
    {
         public DateTimeOffset? RequestFromDate { get; set; }
        public DateTimeOffset? RequestToDate { get; set; }
        public int? RequestType { get; set; }
        public int? RequestStatus { get; set; }                
        public int? SparesTypeId { get; set; }
        public string? SparesType { get; set; } 
        public int? ServiceTypeId { get; set; }
        public string? ServiceType { get; set; }  
        public int? ServiceLocationId { get; set; }
        public string? ServiceLocation { get; set; }
        public string? OldVendorName { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public int? DepartmentId { get; set; }
        public int? MachineId { get; set; }
        public decimal? EstimatedSpareCost { get; set; }
        public decimal? EstimatedServiceCost { get; set; }        
        
        
            
    



    }
}