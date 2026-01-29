using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById
{
    public class GetMachineGroupNameByIdQuery   :   IRequest<List<GetMachineGroupNameByIdDto>>
    {
       public int   ActivityId { get; set; }    
        
    }
}