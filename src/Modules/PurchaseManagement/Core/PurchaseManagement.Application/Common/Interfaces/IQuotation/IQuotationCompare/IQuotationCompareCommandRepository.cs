using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare
{
    public interface IQuotationCompareCommandRepository
    {
        Task<int> AddAsync(QuotationComparisonHeader entity);
        Task<QuoteComparisonWorkFlowDto> GetByIdQuoteComparisonWorkFlowAsync(int id);
        Task<bool> UpdateQuoteApproveAsync(int id, int statusId, CancellationToken ct = default);        
        
    }
}