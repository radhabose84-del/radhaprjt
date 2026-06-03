using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Location.Queries.GetAllLocation
{
    public class GetAllLocationQuery : IRequest<ApiResponseDTO<List<GetAllLocationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
