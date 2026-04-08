using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.ILotMaster
{
    public interface ILotMasterQueryRepository
    {
        Task<(List<LotMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<LotMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<LotMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string lotCode);
        Task<bool> NotFoundAsync(int id);
        Task<bool> LotTypeExistsAsync(int lotTypeId);
        Task<bool> StatusExistsAsync(int statusId);
        Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);
        Task<bool> UnitExistsAsync(int unitId, CancellationToken ct = default);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsLotMasterLinkedAsync(int id);
    }
}
