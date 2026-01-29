using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete
{
    public class GetMachineMasterAutoCompleteQuery : IRequest<List<MachineMasterAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}