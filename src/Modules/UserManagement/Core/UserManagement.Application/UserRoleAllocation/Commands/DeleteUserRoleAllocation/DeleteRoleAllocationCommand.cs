using MediatR;

namespace UserManagement.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation
{
    public class DeleteRoleAllocationCommand : IRequest<int>
    {    
        public int RoleAllocationId { get; set; }

        public DeleteRoleAllocationCommand(int roleAllocationId)
        {
            RoleAllocationId = roleAllocationId;
        }
    }
}