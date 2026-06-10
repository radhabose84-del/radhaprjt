using Contracts.Dtos.Lookups.QC;

namespace Contracts.Interfaces.Lookups.QC
{
    /// <summary>
    /// Cross-module lookup for the QC.MiscMaster master (e.g. QC status values).
    /// Lets Purchase (Arrival/GRN) resolve a QcStatusId to its name without a cross-module JOIN.
    /// Cached automatically by AddLookupCaching() (interface name ends with "Lookup").
    /// </summary>
    public interface IQcMiscMasterLookup
    {
        Task<IReadOnlyList<QcMiscMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

        /// <summary>
        /// Resolves the active QC.MiscMaster Id for a given MiscType code + misc Code
        /// (e.g. miscTypeCode "QP_SOURCE_TYPE", code "GRN"). Null when no match.
        /// </summary>
        Task<int?> GetIdByTypeAndCodeAsync(string miscTypeCode, string code, CancellationToken ct = default);
    }
}
