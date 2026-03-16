using MediatR;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping
{
    public class DeleteRoleItemGroupMappingCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
