using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Users.Queries.GetUsers
{
    public class GetUserQuery : IRequest<ApiResponseDTO<List<UserDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}