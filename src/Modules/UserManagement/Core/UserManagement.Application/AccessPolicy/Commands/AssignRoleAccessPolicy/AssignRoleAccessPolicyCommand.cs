using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy
{
    public class AssignRoleAccessPolicyCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
        public int AccessPolicyId { get; set; }
        public int RoleId         { get; set; }
        public int ValueId        { get; set; }
    }
}
