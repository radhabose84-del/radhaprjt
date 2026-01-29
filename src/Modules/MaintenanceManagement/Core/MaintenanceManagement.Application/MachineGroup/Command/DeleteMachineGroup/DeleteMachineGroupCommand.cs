using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup
{
    public class DeleteMachineGroupCommand : IRequest<bool>
    {
         public int Id { get; set; }
    }
}