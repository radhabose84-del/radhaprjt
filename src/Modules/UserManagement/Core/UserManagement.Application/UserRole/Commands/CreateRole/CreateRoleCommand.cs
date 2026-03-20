using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using MediatR;

namespace UserManagement.Application.UserRole.Commands.CreateRole
{
    public class CreateRoleCommand : IRequest<UserRoleDto>
    {
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public List<RoleItemGroupMappingInputDto>? RoleItemGroupMappings { get; set; }
    }
}