using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using Contracts.Common;

namespace UserManagement.Application.UserRole.Commands.UpdateRole
{
    public class UpdateRoleCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public bool BypassDataAccess { get; set; }
        public byte IsActive { get; set; }
        public List<RoleItemGroupMappingInputDto>? RoleItemGroupMappings { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
