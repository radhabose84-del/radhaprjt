using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IReturnReason;

public interface IReturnReasonQueryRepository
{
    Task<ReturnReasonDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<ReturnReasonDto> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct);
    Task<IReadOnlyList<ReturnReasonLookupDto>> AutocompleteAsync(string? term, CancellationToken ct);
    Task<IReadOnlyList<ReturnReasonLookupDto>> GetByReturnTypeIdAsync(int returnTypeId, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string code, int returnTypeId, int? excludeId = null);
    Task<bool> NotFoundAsync(int id);
    Task<bool> ReturnTypeExistsAsync(int returnTypeId);
    Task<bool> SoftDeleteValidationAsync(int id);
    Task<bool> BelongsToReturnTypeAsync(int returnReasonId, int returnTypeId);
}
