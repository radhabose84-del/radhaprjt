using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete
{
    public class GetMachineGroupAutoCompleteQuery :  IRequest<List<GetMachineGroupAutoCompleteDto>>
    {
         public string? SearchPattern { get; set; }
    }
}