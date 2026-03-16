using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingByRoleId
{
    public class GetRoleItemGroupMappingByRoleIdQuery : IRequest<List<RoleItemGroupMappingLookupDto>>
    {
        public int RoleId { get; set; }
    }
}
