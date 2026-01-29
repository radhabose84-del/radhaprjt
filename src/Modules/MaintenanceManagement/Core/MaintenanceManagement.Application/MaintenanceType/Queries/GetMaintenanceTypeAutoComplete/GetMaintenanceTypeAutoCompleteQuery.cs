using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeAutoComplete
{
    public class GetMaintenanceTypeAutoCompleteQuery : IRequest<List<MaintenanceTypeAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}