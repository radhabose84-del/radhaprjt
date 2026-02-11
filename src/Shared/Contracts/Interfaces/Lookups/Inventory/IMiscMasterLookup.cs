using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IMiscMasterLookup
    {
        Task<List<MiscMasterLookupDto>> GetMiscMasterByIdAsync(string miscType);
        Task<(int? WarehouseTypeId, int? StorageTypeId, int? AreaTypeId, int? OperationTypeId, int? FloorTypeId, int? AisleTypeId, int? RackLevelTypeId)> GetMiscTypeIdsAsync();
    }
}
