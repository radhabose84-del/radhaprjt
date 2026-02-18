using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetActivityMasterByIdQuery   :  IRequest<GetActivityMasterByIdDto>
    {
        public int Id { get; set; }
    }
}