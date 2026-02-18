using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries
{
    public class GetFeederSubFeederByIdQuery :  IRequest<List<GetFeederSubFeederDto>>
    {
        public int FeederTypeId { get; set; }
    }
}