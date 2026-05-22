using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserRole.Commands.DeleteRole
{
    public class DeleteRoleCommand :IRequest<int>, IRequirePermission
    
    {
        public int Id { get; set; } 
                
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
