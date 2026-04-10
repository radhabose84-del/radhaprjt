using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales;

public interface ICommissionSplitLookup
{
    Task<IReadOnlyList<CommissionSplitLookupDto>> GetAllCommissionSplitAsync();
}
