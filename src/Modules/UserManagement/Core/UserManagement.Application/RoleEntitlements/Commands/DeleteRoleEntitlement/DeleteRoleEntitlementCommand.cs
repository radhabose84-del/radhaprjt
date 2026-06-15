using Contracts.Common;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement
{
    public class DeleteRoleEntitlementCommand :  IRequest<ApiResponseDTO<RoleEntitlementDto>>, IRequirePermission 
    {
        public int Id { get; set; }                
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
