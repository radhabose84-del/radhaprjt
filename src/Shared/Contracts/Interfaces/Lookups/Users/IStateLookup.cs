using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IStateLookup
    {
        Task<StateLookupDto?> GetByIdAsync(int stateId, CancellationToken ct = default);

        Task<IReadOnlyList<StateLookupDto>> GetByIdsAsync(IEnumerable<int> stateIds, CancellationToken ct = default);
    }
}
