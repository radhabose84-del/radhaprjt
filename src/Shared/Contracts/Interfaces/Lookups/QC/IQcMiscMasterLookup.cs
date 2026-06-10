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
    }
}
