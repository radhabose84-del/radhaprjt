using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;

public interface IPurchaseOrderQueryRepository
{
    Task<PagedResult<PurchaseOrderListItemDto>> GetAllAsync(int page, int size, string? search,int? poMethodId,int? statusId, int? budgetGroupId, CancellationToken ct);
    Task<PurchaseOrderDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IEnumerable<AutocompleteDto>> GetAutocompleteAsync(string? term, int? poMethodId, int? budgetGroupId, CancellationToken ct);
    Task<(List<GetPOLocalPendingGroupDto> Rows, int Total)> GetPOPendingAsync(int? page, int? size, string? search, int? poId, int? poMethodId, CancellationToken ct);
    Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct);
    Task<bool> ExistsAsync(int poId, CancellationToken ct);
    Task<string?> GetStatusCodeAsync(int poId, CancellationToken ct);
    Task<int> GetNextRevisionAsync(int rootPoId, CancellationToken ct);
    Task<List<LastPoPriceDto>> LastPOPriceByItemIdAsync(List<int> itemIds);
    Task<bool> SoftDeleteValidationAsync(int id);
    Task<decimal> GetTotalPurchaseValueAsync(int? budgetGroupId, int? itemCategoryId, DateTimeOffset poDate, CancellationToken ct);
}
