using MediatR;
using Contracts.Common;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping
{
    public class DeleteRoleItemGroupMappingCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
