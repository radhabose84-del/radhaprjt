using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries
{
    public class GetFeederSubFeederByIdQuery :  IRequest<List<GetFeederSubFeederDto>>
    {
        public int FeederTypeId { get; set; }
    }
}