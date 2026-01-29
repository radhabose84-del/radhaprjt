using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IMiscMasterGrpcClient
    {
       // Task<List<MiscMasterDto>> GetMiscMasterByIdAsync(string miscType);

        Task<List<MiscMasterDto>> GetMiscMasterByIdAsync(string miscType);
        Task<(int? WarehouseTypeId, int? StorageTypeId, int? AreaTypeId, int? OperationTypeId , int? FloorTypeId ,int? AisleTypeId, int? RackLevelTypeId )> GetMiscTypeIdsAsync();
    }
}