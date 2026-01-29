using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest
{
    public class GetMaintenanceRequestQuery :IRequest<ApiResponseDTO<List<GetMaintenanceRequestDto>>>
    {

         public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public DateTimeOffset FromDate { get; set; } 
        public DateTimeOffset ToDate { get; set;}
        
    }
}