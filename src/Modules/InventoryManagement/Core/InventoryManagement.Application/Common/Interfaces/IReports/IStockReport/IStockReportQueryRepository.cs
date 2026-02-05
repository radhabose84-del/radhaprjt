using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Users;
using InventoryManagement.Application.Reports.StockReport;
using InventoryManagement.Application.Reports.StockReportDivisionwise;
using InventoryManagement.Application.Reports.SubStoresStock;

namespace InventoryManagement.Application.Common.Interfaces.IReports.IStockReport
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


        Task<List<StockSummaryDivsionwiseDto>> GetStockReportDivisionSummaryAsync(
            List<int> unitIds,
            int? itemId = null,
            int? warehouseId = null,
            int? storageTypeId = null,
            int? targetId = null);

       
    }
}