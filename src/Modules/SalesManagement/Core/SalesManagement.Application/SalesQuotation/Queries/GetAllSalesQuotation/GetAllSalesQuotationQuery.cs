using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotation
{
    public class GetAllSalesQuotationQuery : IRequest<ApiResponseDTO<List<SalesQuotationHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
