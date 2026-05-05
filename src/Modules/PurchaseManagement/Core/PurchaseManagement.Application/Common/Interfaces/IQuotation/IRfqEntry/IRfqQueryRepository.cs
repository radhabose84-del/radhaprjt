
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry
{
    public interface IRfqQueryRepository
    {
        Task<RfqMaster?> GetAggregateAsync(int id, CancellationToken ct = default, bool excludeQuotation = false);


        Task<(IReadOnlyList<RfqListItemDto> Items, int Total)>  GetAllAsync(int page, int pageSize, int? statusId, string? searchTerm, CancellationToken ct);
        Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteAsync(string? searchPattern, DateOnly? lastSubmitDate, CancellationToken ct);
        Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteQuotationAsync(string? searchPattern, DateOnly? lastSubmitDate, CancellationToken ct);
        Task<List<RfqAutoCompleteDto>> GetRfqAutoCompleteComparisonAsync(string? searchPattern, DateOnly? lastSubmitDate,   int? statusId,    CancellationToken ct);
        
        Task<IReadOnlyList<OpenRfqConflictDto>> FindBlockingSupplierItemPairsAsync(
        IEnumerable<int> itemIds,
        IEnumerable<int> supplierIds,
        int unitId,
        DateOnly now,
        int? excludingRfqId = null,
        CancellationToken ct = default);
    }
}
