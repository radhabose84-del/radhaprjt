using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    /// <summary>
    /// Cross-module read access to Arrival (bale inward) line items for QC inspection.
    /// Returns Purchase-owned data only; QC-required and eligibility filtering
    /// are composed by the consuming (QC) module. Mirror of <see cref="IGrnLookup"/>.
    /// </summary>
    public interface IArrivalLookup
    {
        /// <summary>Single Arrival line snapshot, used when creating an inspection.</summary>
        Task<ArrivalLookupDto?> GetByArrivalDetailIdAsync(int arrivalDetailId, CancellationToken ct = default);

        /// <summary>Batch Arrival line snapshots by detail ids, used to enrich inspection list views.</summary>
        Task<IReadOnlyList<ArrivalLookupDto>> GetByArrivalDetailIdsAsync(IEnumerable<int> arrivalDetailIds, CancellationToken ct = default);

        /// <summary>All Arrival lines, optionally filtered by supplier and arrival date range.</summary>
        Task<IReadOnlyList<ArrivalLookupDto>> GetArrivalLinesAsync(
            int? supplierId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct = default);

        /// <summary>Count of Arrival detail lines for an Arrival header — used for header-level QC derivation.</summary>
        Task<int> GetLineCountAsync(int arrivalHeaderId, CancellationToken ct = default);
    }
}
