using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete;

namespace WarehouseManagement.Application.Common.Interfaces.IRackMaster
{
    public interface IRackMasterQueryRepository
    {
        Task<(List<RackMasterDto>, int)> GetAllAsync(int PageNumber, int PageSize, string SearchTerm);
        Task<RackMasterDto> GetByIdAsync(int id);
        Task<bool> RackSlotAlreadyExistsAsync(int warehouseId, int? floorId, int? aisleId, int? rackLevelId, int? id = null);

        Task<List<GetRackMasterAutoCompleteDto>> GetRackMasterAutoCompletes(string searchPattern = null, int? warehouseId = null);

        Task<List<RackMasterDto>> GetRackAsync(); 
         
      
    }
}