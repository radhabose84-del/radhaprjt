using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetAllSalesOrganisation
{
    public class GetAllSalesOrganisationQuery : IRequest<ApiResponseDTO<List<SalesOrganisationDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
