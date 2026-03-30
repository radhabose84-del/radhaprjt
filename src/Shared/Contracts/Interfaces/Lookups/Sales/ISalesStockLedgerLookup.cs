using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.Lookups.Sales
{
    public interface ISalesStockLedgerLookup
    {
        Task<bool> InsertAsync(List<SalesStockLedgerDto> entries, CancellationToken ct = default);
        Task<bool> DeleteByDocAsync(string docType, int docNo, CancellationToken ct = default);

        /// <summary>
        /// Updates StatusId for all packs in the given range within a document,
        /// only if their current StatusId matches <paramref name="currentStatusId"/>.
        /// Used by Repacking to mark source packs as Deleted before inserting new RPK entries.
        /// </summary>
        Task<bool> UpdateStatusByPackRangeAsync(
            string docType, int docNo,
            int startPackNo, int endPackNo,
            int currentStatusId, int newStatusId,
            CancellationToken ct = default);

        /// <summary>
        /// Returns the distinct PackNos in the given range whose status Description = 'Packed'.
        /// Used by Production query repo to filter details that still have available packed stock.
        /// </summary>
        Task<IReadOnlyList<int>> GetPackedPackNosAsync(
            int startPackNo, int endPackNo,
            CancellationToken ct = default);
    }
}
