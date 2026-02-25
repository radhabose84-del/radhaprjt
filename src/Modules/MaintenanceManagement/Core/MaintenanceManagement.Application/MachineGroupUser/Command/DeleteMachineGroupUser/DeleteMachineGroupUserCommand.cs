using MediatR;

namespace MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser
{
    public class DeleteMachineGroupUserCommand  : IRequest<bool>
    {
        public int Id { get; set; }
    }
}