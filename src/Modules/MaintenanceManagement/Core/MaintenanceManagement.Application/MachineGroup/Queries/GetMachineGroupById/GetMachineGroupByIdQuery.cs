using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetMachineGroupByIdQuery   :  IRequest<GetMachineGroupByIdDto>
    {
        public int Id { get; set; }
    }
}