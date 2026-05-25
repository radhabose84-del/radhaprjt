using Contracts.Dtos.Lookups.Maintenance;

namespace Contracts.Interfaces.Lookups.Maintenance
{
    /// <summary>
    /// Read-only lookup for External Maintenance Requests, used by Service PO UI/queries.
    /// </summary>
    public interface IMaintenanceRequestLookup
    {
        /// <summary>
        /// Returns External requests selectable for Service PO linkage —
        /// status IN (Open, InProgress, PartiallyConverted), excluding Closed/FullyConverted.
        /// </summary>
        Task<IReadOnlyList<MaintenanceRequestLookupDto>> GetAvailableForServicePoAsync(
            string? searchTerm, CancellationToken ct = default);

        Task<MaintenanceRequestLookupDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<MaintenanceRequestLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default);
    }
}
