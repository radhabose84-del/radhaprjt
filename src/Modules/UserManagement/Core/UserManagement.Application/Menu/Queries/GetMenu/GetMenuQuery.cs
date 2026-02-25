using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetMenu
{
    public class GetMenuQuery : IRequest<ApiResponseDTO<List<MenuDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}