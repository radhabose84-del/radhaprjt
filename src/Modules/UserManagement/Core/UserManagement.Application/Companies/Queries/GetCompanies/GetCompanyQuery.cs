using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Companies.Queries.GetCompanies
{
    public class GetCompanyQuery : IRequest<ApiResponseDTO<List<GetCompanyDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}