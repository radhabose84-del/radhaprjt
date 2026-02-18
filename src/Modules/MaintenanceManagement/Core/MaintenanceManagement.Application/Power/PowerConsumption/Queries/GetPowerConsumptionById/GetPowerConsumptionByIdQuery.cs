using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumptionById
{
    public class GetPowerConsumptionByIdQuery :IRequest<GetPowerConsumptionDto>
    {
        public int Id { get; set; }
    }
}