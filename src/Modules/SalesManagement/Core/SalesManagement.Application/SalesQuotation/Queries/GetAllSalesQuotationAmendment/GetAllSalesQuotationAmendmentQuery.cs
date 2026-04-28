using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotationAmendment
{
    public class GetAllSalesQuotationAmendmentQuery : IRequest<ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
