using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;

public interface IPurchaseReturnQueryRepository
{
    Task<PurchaseReturnHeaderDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<PurchaseReturnListItemDto> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct);
    Task<IReadOnlyList<PurchaseReturnListItemDto>> AutocompleteAsync(string? term, CancellationToken ct);
    Task<IReadOnlyList<ReturnableQtyDto>> GetReturnableQtyByGrnAsync(int grnHeaderId, CancellationToken ct);
    Task<bool> NotFoundAsync(int id);
    Task<int?> GetStatusIdByCodeAsync(string statusCode);
    Task<string?> GetCurrentStatusCodeAsync(int id);
    Task<string?> GetReturnTypeApprovalRoleCodeAsync(int returnTypeId);
}
