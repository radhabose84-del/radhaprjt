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

        /// <summary>
        /// True when any non-deleted ArrivalDetail (excluding the optional header) already overlaps
        /// the given bale range — supports duplicate-range prevention (R3).
        /// </summary>
        Task<bool> BaleRangeOverlapsAsync(long from, long to, int? excludeHeaderId);
    }
}
