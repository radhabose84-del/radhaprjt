using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping
{
    public class UpdateRoleItemGroupMappingCommand : IRequest<RoleItemGroupMappingDto>
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ItemGroupId { get; set; }
        public int IsActive { get; set; }
    }
}
