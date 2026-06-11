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
        /// True when any non-deleted ArrivalDetail under the SAME Raw Material PO (lot) — excluding the
        /// optional header — already overlaps the given bale range. Supports duplicate-range prevention (R3);
        /// bale numbers may repeat across different POs/lots.
        /// </summary>
        Task<bool> BaleRangeOverlapsAsync(long from, long to, int rawMaterialPoId, int? excludeHeaderId);
    }
}
