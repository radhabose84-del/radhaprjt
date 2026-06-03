using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    /// <summary>
    /// Cross-module lookup for the AppData.Station master (active, non-deleted).
    /// </summary>
    public interface IStationLookup
    {
        Task<StationLookupDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<StationLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
