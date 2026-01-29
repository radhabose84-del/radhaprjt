using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster
{
    public class GetMiscMasterQuery :IRequest<ApiResponseDTO<List<GetMiscMasterDto>>>
    {
          
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

        
    }
}