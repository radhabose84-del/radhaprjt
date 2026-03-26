using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales;

public interface ISalesSegmentLookup
{
    Task<IReadOnlyList<SalesSegmentLookupDto>> GetAllSalesSegmentAsync();
}
