using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete
{
    public class GetMachineMasterAutoCompleteQuery : IRequest<List<MachineMasterAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}