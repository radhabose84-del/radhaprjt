using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICountryLookup
    {
        Task<List<CountryLookupDto>> GetAllCountriesAsync(CancellationToken ct = default);

        Task<CountryLookupDto?> GetByIdAsync(int countryId, CancellationToken ct = default);

        Task<IReadOnlyList<CountryLookupDto>> GetByIdsAsync(IEnumerable<int> countryIds, CancellationToken ct = default);
    }
}
