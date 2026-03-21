using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Common
{
    public interface IAppDataMiscMasterLookup
    {
        Task<MiscMasterLookupDto?> GetMiscMasterByNameAsync(string miscTypeCode, string code);
    }
}
