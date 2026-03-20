using Contracts.Common;
using MediatR;
using UserManagement.Application.RoleItemGroupMapping.Dto;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetAllRoleItemGroupMapping
{
    public class GetAllRoleItemGroupMappingQuery : IRequest<ApiResponseDTO<List<RoleItemGroupMappingDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
