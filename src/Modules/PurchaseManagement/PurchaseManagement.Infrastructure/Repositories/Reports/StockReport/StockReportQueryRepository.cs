using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IReports.IStockReport;
using PurchaseManagement.Application.Reports.StockReport;
using PurchaseManagement.Application.Reports.SubStoresStock;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Reports.StockReport
{
    public class StockReportQueryRepository : IStockReportQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public StockReportQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<StockSummaryDto>> GetStockSummaryAsync(int? itemId = null, int? warehouseId = null, int? storageTypeId = null, int? targetId = null)
        {
            var UnitId = _ipAddressService.GetUnitId();
            // Base SQL
            var sql = @"
                SELECT 
                    ItemId,
                    WarehouseId,
                    StorageTypeId,
                    TargetId,
                    UomId,
                    SUM(ReceivedQty - IssueQty) AS CurrentStockQty,
                    SUM(ReceivedValue - IssueValue) AS CurrentStockValue,
                    CASE 
                    WHEN SUM(ReceivedQty - IssueQty) <> 0 
                    THEN SUM(ReceivedValue - IssueValue) / SUM(ReceivedQty - IssueQty)
                    ELSE 0 
                END AS AvgRate
                FROM Purchase.StockLedger
                WHERE UnitId = @UnitId
            ";

            // Optional filters
            if (itemId.HasValue)
                sql += " AND ItemId = @ItemId";

            if (warehouseId.HasValue)
                sql += " AND WarehouseId = @WarehouseId";

            if (storageTypeId.HasValue)
                sql += " AND StorageTypeId = @StorageTypeId";

            if (targetId.HasValue)
                sql += " AND TargetId = @TargetId";

            sql += @"
                GROUP BY UnitId, ItemId, WarehouseId, StorageTypeId, TargetId, UomId
                HAVING SUM(ReceivedQty - IssueQty) <> 0;
            ";

            // Execute Dapper query
            var result = await _dbConnection.QueryAsync<StockSummaryDto>(
                sql,
                new
                {
                    UnitId = UnitId,
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    StorageTypeId = storageTypeId,
                    TargetId = targetId
                }
            );

            return result.ToList();
        }

        public async Task<List<SubStockSummaryDto>> GetSubStoresStockSummaryAsync(int? itemId = null, int? departmentId = null, int? warehouseId = null, int? storageTypeId = null, int? targetId = null)
        {
            var UnitId = _ipAddressService.GetUnitId();
            // Base SQL
            var sql = @"
                
                    SELECT 
                    ItemId,
					DepartmentId,
                    WarehouseId,
                    StorageTypeId,
                    TargetId,
                    UomId,
                    SUM(ReceivedQty - IssueQty) AS CurrentStockQty,
                    SUM(ReceivedValue - IssueValue) AS CurrentStockValue,
                    CASE 
                    WHEN SUM(ReceivedQty - IssueQty) <> 0 
                    THEN SUM(ReceivedValue - IssueValue) / SUM(ReceivedQty - IssueQty)
                    ELSE 0 
                END AS AvgRate
                FROM Purchase.SubStoreStockLedger
                WHERE UnitId = @UnitId
            ";

            // Optional filters
            if (itemId.HasValue)
                sql += " AND ItemId = @ItemId";

            if (departmentId.HasValue)
                sql += " AND DepartmentId = @DepartmentId";

            if (warehouseId.HasValue)
                sql += " AND WarehouseId = @WarehouseId";

            if (storageTypeId.HasValue)
                sql += " AND StorageTypeId = @StorageTypeId";

            if (targetId.HasValue)
                sql += " AND TargetId = @TargetId";

            sql += @"
                GROUP BY UnitId, ItemId, WarehouseId, StorageTypeId, TargetId, UomId, DepartmentId
                HAVING SUM(ReceivedQty - IssueQty) <> 0;
            ";

            // Execute Dapper query
            var result = await _dbConnection.QueryAsync<SubStockSummaryDto>(
                sql,
                new
                {
                    UnitId = UnitId,
                    ItemId = itemId,
                    DepartmentId = departmentId,
                    WarehouseId = warehouseId,
                    StorageTypeId = storageTypeId,
                    TargetId = targetId
                }
            );

            return result.ToList();
        }
    }
}