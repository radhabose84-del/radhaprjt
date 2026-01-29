using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById
{
    public class GetFeederGroupByIdQuery :IRequest<GetFeederGroupByIdDto>
    {

     public int  Id { get; set; }
        
    }
}