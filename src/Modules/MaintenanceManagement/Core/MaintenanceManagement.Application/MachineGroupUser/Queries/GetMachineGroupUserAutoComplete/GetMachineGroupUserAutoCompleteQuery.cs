using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
{
    public class GetMachineGroupUserAutoCompleteQuery : IRequest<List<MachineGroupUserAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}