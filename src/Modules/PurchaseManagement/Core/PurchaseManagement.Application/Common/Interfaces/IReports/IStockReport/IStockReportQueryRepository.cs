using PurchaseManagement.Application.Reports.StockReport;
using PurchaseManagement.Application.Reports.SubStoresStock;

namespace PurchaseManagement.Application.Common.Interfaces.IReports.IStockReport
{
    public interface IStockReportQueryRepository
    {
        Task<List<StockSummaryDto>> GetStockSummaryAsync(
           int? itemId = null,
           int? warehouseId = null,
           int? storageTypeId = null,
           int? targetId = null);


        Task<List<SubStockSummaryDto>> GetSubStoresStockSummaryAsync(
           int? itemId = null,
           int? departmentId = null,
           int? warehouseId = null,
           int? storageTypeId = null,
           int? targetId = null);

     
        
    }
}