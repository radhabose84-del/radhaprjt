using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    /// <summary>
    /// Cross-module lookup for the AppData.Location master (active, non-deleted).
    /// Named distinctly from <c>ILocationLookup</c> (geographic City/State/Country).
    /// </summary>
    public interface ILocationMasterLookup
    {
        Task<LocationMasterLookupDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<LocationMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

        /// <summary>
        /// Reuse-or-insert a location by name (mirrors the City/State/Country GetOrCreate in
        /// <c>ILocationLookup</c>). Returns the existing or newly-created location id. Used when a
        /// party address sends a location name picked from the pincode lookup.
        /// </summary>
        Task<int> GetOrCreateByNameAsync(string locationName, CancellationToken ct = default);
    }
}
