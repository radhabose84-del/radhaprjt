using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete
{
    public class GetMaintenanceCategoryAutoCompleteQuery : IRequest<List<MaintenanceCategoryAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}