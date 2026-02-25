using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete
{
    public class GetMachineMasterAutoCompleteQuery : IRequest<List<MachineMasterAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}