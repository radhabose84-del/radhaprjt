using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById;

namespace MaintenanceManagement.Application.Common.Interfaces.IStcokLedger
{
    public interface IStockLedgerQueryRepository
    {
        Task<CurrentStockDto?> GetSubStoresCurrentStock(string OldUnitcode, string Itemcode, int DepartmentId);
        Task<List<StockItemCodeDto>> GetStockItemCodes(string OldUnitcode, int DepartmentId);
         Task<List<StockItemCodeDto>> GetAllItemCodes(string OldUnitcode,int DepartmentId);
     
         
         
    }
}