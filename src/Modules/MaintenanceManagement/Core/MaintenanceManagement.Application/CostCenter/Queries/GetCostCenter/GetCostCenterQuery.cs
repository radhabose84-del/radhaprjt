using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter
{
    public class GetCostCenterQuery : IRequest<ApiResponseDTO<List<CostCenterDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}