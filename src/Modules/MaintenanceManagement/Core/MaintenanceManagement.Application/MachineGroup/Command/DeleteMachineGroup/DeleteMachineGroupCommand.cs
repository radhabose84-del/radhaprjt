using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup
{
    public class DeleteMachineGroupCommand : IRequest<bool>
    {
         public int Id { get; set; }
    }
}