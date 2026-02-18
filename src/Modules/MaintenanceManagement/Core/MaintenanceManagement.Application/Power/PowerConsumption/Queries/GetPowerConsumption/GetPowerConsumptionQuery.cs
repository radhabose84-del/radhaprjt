using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption
{
    public class GetPowerConsumptionQuery  :  IRequest<ApiResponseDTO<List<GetPowerConsumptionDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}