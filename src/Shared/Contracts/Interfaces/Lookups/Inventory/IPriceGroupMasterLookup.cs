using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IPriceGroupMasterLookup
    {
        Task<IReadOnlyList<PriceGroupMasterLookupDto>> GetAllPriceGroupMasterAsync();
    }
}
