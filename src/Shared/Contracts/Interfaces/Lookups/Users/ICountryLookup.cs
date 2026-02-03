using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICountryLookup
    {
        Task<CountryLookupDto?> GetByIdAsync(int countryId, CancellationToken ct = default);

        Task<IReadOnlyList<CountryLookupDto>> GetByIdsAsync(IEnumerable<int> countryIds, CancellationToken ct = default);
    }
}