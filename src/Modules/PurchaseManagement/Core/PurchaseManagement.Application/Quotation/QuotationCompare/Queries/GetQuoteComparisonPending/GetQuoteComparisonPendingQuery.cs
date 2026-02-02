using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending
{
    public sealed class GetQuoteComparisonPendingQuery 
        : IRequest<(List<QuoteComparisonPendingGroupDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
