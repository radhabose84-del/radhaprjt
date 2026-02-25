using MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems;

namespace MaintenanceManagement.Application.Common.Interfaces.IMainStoreStock
{
    public interface IMainStoreStockQueryRepository
    {
        Task<List<MainStoresStockDto>> GetStockDetails(string OldUnitcode,string GroupCode);
        Task<List<MainStoresStockItemsDto>> GetStockItemsCodes(string OldUnitcode,string GroupCode);
        Task<MainStoreItemStockDto?> GetByItemCodeIdAsync(string OldUnitcode,string ItemCode);
    }
}