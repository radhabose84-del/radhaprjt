using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales
{
    public interface ISalesMiscMasterLookup
    {
        Task<SalesMiscMasterLookupDto?> GetByCodeAsync(string code);
    }
}
