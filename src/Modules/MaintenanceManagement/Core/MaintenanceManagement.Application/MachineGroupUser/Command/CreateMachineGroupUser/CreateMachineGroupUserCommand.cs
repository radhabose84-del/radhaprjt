using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser
{
    public class CreateMachineGroupUserCommand  :IRequest<int>, IRequirePermission
    {
        public int MachineGroupId { get; set; }
        public int DepartmentId { get; set; }
        public int UserId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
