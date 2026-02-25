using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICityLookup
    {
        Task<List<CityLookupDto>> GetAllCityAsync(CancellationToken ct = default);

        Task<CityLookupDto?> GetByIdAsync(int cityId, CancellationToken ct = default);

        Task<IReadOnlyList<CityLookupDto>> GetByIdsAsync(IEnumerable<int> cityIds, CancellationToken ct = default);
    }
}
