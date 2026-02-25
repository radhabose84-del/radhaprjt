using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Divisions.Queries.GetDivisions
{
    public class GetDivisionQuery : IRequest<ApiResponseDTO<List<DivisionDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}