using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumptionById
{
    public class GetPowerConsumptionByIdQuery :IRequest<GetPowerConsumptionDto>
    {
        public int Id { get; set; }
    }
}