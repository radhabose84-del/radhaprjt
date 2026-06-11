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
        /// Total ordered (PO) quantity per ItemId for the given Raw Material PO header, keyed by ItemId.
        /// Used to validate that an arrival line's ArrivedQty does not exceed the PO quantity.
        /// </summary>
        Task<IReadOnlyDictionary<int, decimal>> GetRawMaterialPOItemQuantitiesAsync(int rawMaterialPOId);

        /// <summary>Most recent lot number (= latest ArrivalHeader Id) for the current unit, or null when none exists.</summary>
        Task<ArrivalLastLotNoDto?> GetLastLotNoAsync();

        /// <summary>
        /// Remaining (balance) quantity per item for a Raw Material PO, scoped to the current unit:
        /// PO ordered qty − total arrived qty. One row per PO item.
        /// </summary>
        Task<IReadOnlyList<ArrivalBalanceQtyDto>> GetBalanceQuantitiesAsync(int rawMaterialPOId);
    }
}
