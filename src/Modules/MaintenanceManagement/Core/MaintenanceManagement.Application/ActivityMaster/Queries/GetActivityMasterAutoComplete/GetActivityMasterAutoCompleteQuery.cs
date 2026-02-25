using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetActivityMasterAutoComplete
{
    public class GetActivityMasterAutoCompleteQuery : IRequest<List<GetActivityMasterAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
         
         public string? MachineCode { get; set; }
    }
}