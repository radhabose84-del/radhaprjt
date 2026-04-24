using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.ILotMaster
{
    public interface ILotMasterQueryRepository
    {
        Task<(List<LotMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? itemId = null);
        Task<LotMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<LotMasterLookupDto>> AutocompleteAsync(string term, int? itemId, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string lotCode, int unitId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> LotTypeExistsAsync(int lotTypeId);
        Task<bool> StatusExistsAsync(int statusId);
        Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);
        Task<bool> UnitExistsAsync(int unitId, CancellationToken ct = default);
        Task<bool> VariantExistsAsync(int variantId, CancellationToken ct = default);
        Task<bool> VariantBelongsToItemAsync(int variantId, int itemId, CancellationToken ct = default);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsLotMasterLinkedAsync(int id);
    }
}
