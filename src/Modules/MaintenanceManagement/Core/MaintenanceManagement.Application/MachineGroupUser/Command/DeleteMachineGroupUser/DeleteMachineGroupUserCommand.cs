using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser
{
    public class DeleteMachineGroupUserCommand  : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
