using Contracts.Common;
using MediatR;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetAllMarketingOfficer
{
    public class GetAllMarketingOfficerQuery : IRequest<ApiResponseDTO<List<MarketingOfficerDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
