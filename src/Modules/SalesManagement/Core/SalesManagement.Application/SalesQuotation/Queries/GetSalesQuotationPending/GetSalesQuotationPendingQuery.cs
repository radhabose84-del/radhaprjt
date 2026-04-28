using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationPending
{
    public sealed class GetSalesQuotationPendingQuery
        : IRequest<(List<GetSalesQuotationPendingDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
