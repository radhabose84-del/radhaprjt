using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete
{
    public class GetMaintenanceCategoryAutoCompleteQuery : IRequest<List<MaintenanceCategoryAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}