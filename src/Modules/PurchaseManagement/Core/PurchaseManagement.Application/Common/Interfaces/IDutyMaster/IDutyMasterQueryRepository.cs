using PurchaseManagement.Application.DutyMaster;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster
{
    public interface IDutyMasterQueryRepository
    {
        Task<(IReadOnlyList<PurchaseManagement.Domain.Entities.DutyMaster> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? search, CancellationToken ct);
        Task<PurchaseManagement.Domain.Entities.DutyMaster?> GetByIdAsync(int id, CancellationToken ct);
        Task<bool> ExistsAsync(string dutyCode, string tariffNumber, DateTimeOffset effectiveFrom, CancellationToken ct);
        Task<IReadOnlyList<DutyMasterAutocompleteDto>> GetAutocompleteAsync(string? term, CancellationToken ct);
        Task<string> GenerateDutyCodeAsync(CancellationToken ct);
        Task<IReadOnlyList<DutyMasterReadDto>> GetByTariffOrHsnAsync(
        IEnumerable<string> tariffNumbers,
        IEnumerable<string> hsnCodes,
        DateTimeOffset onDate,
        CancellationToken ct = default);
    }
}


