using Contracts.Common;
using MediatR;

namespace UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy
{
    public sealed record DeleteAccessPolicyCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
