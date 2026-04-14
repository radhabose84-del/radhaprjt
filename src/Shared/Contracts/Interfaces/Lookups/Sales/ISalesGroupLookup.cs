using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales;

public interface ISalesGroupLookup
{
    Task<IReadOnlyList<SalesGroupLookupDto>> GetAllSalesGroupAsync();
}
