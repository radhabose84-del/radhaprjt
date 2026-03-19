using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingById
{
    public class GetRoleItemGroupMappingByIdQuery : IRequest<RoleItemGroupMappingDto>
    {
        public int Id { get; set; }
    }
}
