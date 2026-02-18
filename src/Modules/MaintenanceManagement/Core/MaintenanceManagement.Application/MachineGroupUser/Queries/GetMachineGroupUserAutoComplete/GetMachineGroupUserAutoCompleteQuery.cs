using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
{
    public class GetMachineGroupUserAutoCompleteQuery : IRequest<List<MachineGroupUserAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}