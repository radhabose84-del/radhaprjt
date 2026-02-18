using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete
{
    public class GetMachineGroupAutoCompleteQuery :  IRequest<List<GetMachineGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}