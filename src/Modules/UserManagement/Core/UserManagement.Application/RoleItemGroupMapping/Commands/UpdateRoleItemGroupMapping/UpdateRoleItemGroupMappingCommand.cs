using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using Contracts.Common;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping
{
    public class UpdateRoleItemGroupMappingCommand : IRequest<RoleItemGroupMappingDto>, IRequirePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ItemGroupId { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
