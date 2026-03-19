using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping
{
    public class CreateRoleItemGroupMappingCommand : IRequest<RoleItemGroupMappingDto>
    {
        public int RoleId { get; set; }
        public int ItemGroupId { get; set; }
    }
}
