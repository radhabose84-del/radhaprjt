using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserRole.Queries.GetRole
{
    public class GetRoleQuery : IRequest<ApiResponseDTO<List<GetUserRoleDto>>>
    {

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

        
    }
}