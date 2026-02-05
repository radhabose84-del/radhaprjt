using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICityLookup
    {
        Task<CityLookupDto?> GetByIdAsync(int cityId, CancellationToken ct = default);

        Task<IReadOnlyList<CityLookupDto>> GetByIdsAsync(IEnumerable<int> cityIds, CancellationToken ct = default);
    }
}
