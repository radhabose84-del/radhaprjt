using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    /// <summary>
    /// Cross-module read access to GRN line items for QC inspection.
    /// Returns Purchase-owned data only; QC-required and eligibility filtering
    /// are composed by the consuming (QC) module.
    /// </summary>
    public interface IGrnLookup
    {
        /// <summary>Single GRN line snapshot, used when creating an inspection.</summary>
        Task<GrnLookupDto?> GetByGrnDetailIdAsync(int grnDetailId, CancellationToken ct = default);

        /// <summary>Batch GRN line snapshots by detail ids, used to enrich inspection list views.</summary>
        Task<IReadOnlyList<GrnLookupDto>> GetByGrnDetailIdsAsync(IEnumerable<int> grnDetailIds, CancellationToken ct = default);

        /// <summary>All GRN lines for generated GRNs, optionally filtered by supplier and GRN date range.</summary>
        Task<IReadOnlyList<GrnLookupDto>> GetGrnLinesAsync(
            int? supplierId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct = default);

        /// <summary>Count of GRN detail lines for a GRN header — used for GRN-level QC status derivation.</summary>
        Task<int> GetLineCountAsync(int grnHeaderId, CancellationToken ct = default);
    }
}
