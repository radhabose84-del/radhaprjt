using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy
{
    public sealed record RemoveRoleAccessPolicyCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
