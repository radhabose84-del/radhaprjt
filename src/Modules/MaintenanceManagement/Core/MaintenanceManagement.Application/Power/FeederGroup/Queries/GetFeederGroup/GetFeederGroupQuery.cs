using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup
{
    public class GetFeederGroupQuery :  IRequest<ApiResponseDTO<List<FeederGroupDto>>>

    {
        
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}