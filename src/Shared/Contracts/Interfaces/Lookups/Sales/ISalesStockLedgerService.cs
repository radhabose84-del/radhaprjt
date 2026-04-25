using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.Lookups.Sales
{
    /// <summary>
    /// Non-cached service for all Sales.StockLedger operations.
    /// Intentionally NOT named with "Lookup" suffix to bypass the global CachedLookupDecorator,
    /// because StockLedger is a high-frequency transactional table — caching any of its methods
    /// would return stale data after inserts/updates/deletes.
    /// </summary>
    public interface ISalesStockLedgerService
    {
        Task<bool> InsertAsync(List<SalesStockLedgerDto> entries, CancellationToken ct = default);

        Task<bool> DeleteByDocAsync(string docType, int docNo, int productionYear, int unitId, CancellationToken ct = default);

        /// <summary>
        /// Updates StatusId for all packs in the given range,
        /// only if their current StatusId matches <paramref name="currentStatusId"/>.
        /// </summary>
        Task<bool> UpdateStatusByPackRangeAsync(
            string docType,
            int startPackNo, int endPackNo,
            int currentStatusId, int newStatusId,
            int productionYear, int unitId,
            CancellationToken ct = default);

        /// <summary>
        /// Returns the distinct PackNos in the given range whose status Description = 'Packed'.
        /// </summary>
        Task<IReadOnlyList<int>> GetPackedPackNosAsync(
            int startPackNo, int endPackNo,
            int productionYear, int unitId,
            CancellationToken ct = default);

        Task<IReadOnlyList<StockItemSummaryDto>> GetStockItemsAsync(
            int productionYear, int unitId,
            CancellationToken ct = default);

        /// <summary>
        /// Returns MAX(PackNo) from StockLedger where YEAR(DocDate) = productionYear and UnitId = unitId.
        /// Always reads from the database — never cached.
        /// </summary>
        Task<int> GetLastPackNoByYearAsync(int productionYear, int unitId, CancellationToken ct = default);

        /// <summary>
        /// Returns the LotId of the first pack found in the given pack range for a specific year and unit.
        /// </summary>
        Task<int> GetLotIdByPackRangeAsync(int startPackNo, int endPackNo, int productionYear, int unitId, CancellationToken ct = default);

        /// <summary>
        /// Returns the LotId, WarehouseId, and BinId of the first packed pack found in the given range.
        /// Used to auto-populate OldLotId, OldWarehouseId, OldBinId on RepackingDetail from source packs.
        /// </summary>
        Task<StockPackSourceDto?> GetPackSourceInfoAsync(
            int startPackNo, int endPackNo, int productionYear, int unitId,
            CancellationToken ct = default);

        /// <summary>
        /// Two-mode query for packed packs filtered by ItemId.
        /// When lotId is NULL: returns lot-level summary (grouped by LotId) with LotCode, BatchNumber, TotalBags, NetWeight.
        /// When lotId is provided: returns pack-type-level details for that lot with PackType info + TareWeight, GrossWeight, ConesPerBag.
        /// </summary>
        Task<IReadOnlyList<StockPackSummaryDto>> GetPacksByItemAndLotAsync(
            int itemId, int? lotId, int productionYear, int unitId,
            CancellationToken ct = default);

        /// <summary>
        /// Returns packed stock entries (PackNo, ItemId, LotId, PackTypeId) for a given ItemId.
        /// Filters by UnitId (from token) and optional SourceUnitId.
        /// </summary>
        Task<IReadOnlyList<StockLotByItemDto>> GetLotByStockAsync(
            int itemId, int? sourceUnitId, int unitId,
            CancellationToken ct = default);
    }
}
