using MediatR;
using Contracts.Common;

namespace UserManagement.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation
{
    public class DeleteRoleAllocationCommand : IRequest<int>, IRequirePermission
    {    
        public int RoleAllocationId { get; set; }

        public DeleteRoleAllocationCommand(int roleAllocationId)
        {
            RoleAllocationId = roleAllocationId;
        }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
