using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;

namespace WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster
{
    public interface IWarehouseMasterQueryRepository
    {

        Task<(List<WarehouseMasterDto>, int)> GetAllAsync(int PageNumber, int PageSize, string SearchTerm);

        Task<WarehouseMasterDto> GetByIdAsync(int id);

        Task<bool> ExistsByNameAsync(string warehouseName, int? excludeId = null);

        Task<List<GetWarehouseAutoCompleteDto>> GetWarehouseMasterAutoCompletes(string searchPattern);

        Task<List<GetParentWarehouseDto>> GetParentWarehouseMaster();

        Task<List<WarehouseMasterDto>> GetwarehouseAsync();
    }
}