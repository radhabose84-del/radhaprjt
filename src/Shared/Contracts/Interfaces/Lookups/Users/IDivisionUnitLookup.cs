using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IDivisionUnitLookup
    {
        Task<List<DivisionUnitLookupDto>> GetUnitsByDivisionAsync(
            int companyId,
            int divisionId,
            CancellationToken ct = default);
    }
}
