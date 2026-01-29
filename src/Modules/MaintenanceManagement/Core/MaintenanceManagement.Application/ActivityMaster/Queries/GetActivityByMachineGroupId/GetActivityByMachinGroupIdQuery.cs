using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId
{
    public class GetActivityByMachinGroupIdQuery :IRequest<List<GetActivityByMachineGroupDto>>
    {

     public int MachineGroupId { get; set; }
        
    }
}