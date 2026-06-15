using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserRole.Commands.CreateRole
{
    public class CreateRoleCommand : IRequest<UserRoleDto>, IRequirePermission
    {
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public bool BypassDataAccess { get; set; }
        public List<RoleItemGroupMappingInputDto>? RoleItemGroupMappings { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
