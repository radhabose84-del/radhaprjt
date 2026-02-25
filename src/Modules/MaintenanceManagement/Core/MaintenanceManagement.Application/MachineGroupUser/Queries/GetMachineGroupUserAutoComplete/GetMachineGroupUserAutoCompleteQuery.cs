using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete
{
    public class GetMachineGroupUserAutoCompleteQuery : IRequest<List<MachineGroupUserAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}