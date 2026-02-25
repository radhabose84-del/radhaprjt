using Contracts.Dtos.Lookups.Budget;

namespace Contracts.Interfaces.Lookups.Budget;

public interface IBudgetGroupLookup
{
    Task<IReadOnlyList<BudgetGroupLookupDto>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken ct = default);
}
