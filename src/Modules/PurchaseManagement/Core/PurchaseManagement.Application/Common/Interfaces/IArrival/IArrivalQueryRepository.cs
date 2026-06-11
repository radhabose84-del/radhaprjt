using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IArrival
{
    public interface IArrivalQueryRepository
    {
        Task<(List<ArrivalDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, bool? pendingStatus = null);
        Task<ArrivalDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ArrivalLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> NotFoundAsync(int id);

        // ── FK existence (same-module) ──
        Task<bool> RawMaterialPOExistsAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
    }
}
