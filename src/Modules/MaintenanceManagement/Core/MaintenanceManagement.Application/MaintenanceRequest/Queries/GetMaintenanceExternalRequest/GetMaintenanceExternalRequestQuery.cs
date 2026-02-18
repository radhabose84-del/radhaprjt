using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest
{
    public class GetMaintenanceExternalRequestQuery :IRequest<ApiResponseDTO<List<GetMaintenanceExternalRequestDto>>>
    {
         public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
         public DateTimeOffset FromDate { get; set; } 
        public DateTimeOffset ToDate { get; set;}

    }
}