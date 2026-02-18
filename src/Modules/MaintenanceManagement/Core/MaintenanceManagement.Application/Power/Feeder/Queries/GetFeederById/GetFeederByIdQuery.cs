using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById
{
    public class GetFeederByIdQuery : IRequest<GetFeederByIdDto>
    {
        public int  Id { get; set; }        

    }
}