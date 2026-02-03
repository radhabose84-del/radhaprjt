using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IUnitLookup
    {
        Task<UnitLookupDto?> GetByIdAsync(int unitId, CancellationToken ct = default);
        Task<IReadOnlyList<UnitLookupDto>> GetByIdsAsync(IEnumerable<int> unitIds, CancellationToken ct = default);

    }
}