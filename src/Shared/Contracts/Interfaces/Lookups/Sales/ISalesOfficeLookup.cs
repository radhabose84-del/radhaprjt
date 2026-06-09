using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales;

public interface ISalesOfficeLookup
{
    Task<IReadOnlyList<SalesOfficeLookupDto>> GetAllSalesOfficeAsync();
}
