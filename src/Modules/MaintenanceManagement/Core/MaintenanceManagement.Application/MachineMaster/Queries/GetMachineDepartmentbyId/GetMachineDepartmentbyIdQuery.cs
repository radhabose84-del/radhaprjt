using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId
{
    public class GetMachineDepartmentbyIdQuery : IRequest<MachineDepartmentGroupDto>
    {
        public int MachineGroupId { get; set; }
    }
}