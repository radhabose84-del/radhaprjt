using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare
{
    public interface IQuotationCompareQueryRepository
    {
        Task<QuoteComparisonDto?> GetQuoteComparisonAsync(int rfqId, CancellationToken cancellationToken);
        Task<(List<QuoteComparisonPendingGroupDto>, int)> GetQuoteComparisonPendingAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<QuoteCompareByIdDto> GetByIdQuoteCompareAsync(int id, CancellationToken ct=default);
    }
}