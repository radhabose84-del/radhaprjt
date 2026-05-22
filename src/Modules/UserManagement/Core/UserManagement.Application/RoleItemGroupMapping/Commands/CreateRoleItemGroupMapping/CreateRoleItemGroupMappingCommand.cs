using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using Contracts.Common;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping
{
    public class CreateRoleItemGroupMappingCommand : IRequest<RoleItemGroupMappingDto>, IRequirePermission
    {
        public int RoleId { get; set; }
        public int ItemGroupId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
