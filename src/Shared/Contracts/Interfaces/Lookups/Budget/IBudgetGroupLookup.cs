using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Budget;

namespace Contracts.Interfaces.Lookups.Budget;

public interface IBudgetGroupLookup
{
    Task<IReadOnlyList<BudgetGroupLookupDto>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken ct = default);
}
