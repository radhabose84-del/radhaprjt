using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IReturnType;

public interface IReturnTypeQueryRepository
{
    Task<ReturnTypeDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<ReturnTypeDto> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct);
    Task<IReadOnlyList<ReturnTypeLookupDto>> AutocompleteAsync(string? term, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string code, int? excludeId = null);
    Task<bool> NotFoundAsync(int id);
    Task<bool> InventoryImpactExistsAsync(int miscMasterId);
    Task<bool> FinanceImpactExistsAsync(int miscMasterId);
    Task<bool> SoftDeleteValidationAsync(int id);
}
