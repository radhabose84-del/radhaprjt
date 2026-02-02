using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;

public interface IQuotationQueryRepository
{    
    Task<(List<QuotationListItemDto> Items, int Total)> GetAllAsync(int PageNumber, int PageSize, string? SearchTerm);
    Task<GetQuotationHeaderDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<QuotationAutoCompleteDto>> GetQuotationAutoComplete(string? searchPattern);
  
}
