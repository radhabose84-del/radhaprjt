using System.Data;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IReports.IStockReport;
using InventoryManagement.Application.Reports.StockReport;
using InventoryManagement.Application.Reports.StockReportDivisionwise;
using InventoryManagement.Application.Reports.SubStoresStock;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Reports
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

       public async Task<List<StockSummaryDivsionwiseDto>> GetStockReportDivisionSummaryAsync(
                    List<int> unitIds,
                    int? itemId = null,
                    int? warehouseId = null,
                    int? storageTypeId = null,
                    int? targetId = null)
            {
                var sql = @"
                    SELECT 
                        SL.UnitId,

                        SL.ItemId,
                        IM.ItemCode,
                        IM.ItemName,

                        SL.WarehouseId,
                        SL.StorageTypeId,
                        SL.TargetId,

                        SL.UomId,
                        U.UomName,

                        SUM(SL.ReceivedQty - SL.IssueQty) AS CurrentStockQty,
                        SUM(SL.ReceivedValue - SL.IssueValue) AS CurrentStockValue,

                        CASE 
                            WHEN SUM(SL.ReceivedQty - SL.IssueQty) <> 0 
                            THEN SUM(SL.ReceivedValue - SL.IssueValue) 
                                / SUM(SL.ReceivedQty - SL.IssueQty)
                            ELSE 0 
                        END AS AvgRate,

                        MM.Description AS IssueRuleName

                    FROM Inventory.StockLedger SL
                    INNER JOIN Inventory.ItemMaster IM 
                        ON IM.Id = SL.ItemId
                    INNER JOIN Inventory.UOM U 
                        ON U.Id = SL.UomId
                    INNER JOIN Inventory.MiscMaster MM
                        ON IM.IssueRuleId = MM.Id

                    WHERE SL.UnitId IN @UnitIds
                    AND (@ItemId IS NULL OR SL.ItemId = @ItemId)
                    AND (@WarehouseId IS NULL OR SL.WarehouseId = @WarehouseId)
                    AND (@StorageTypeId IS NULL OR SL.StorageTypeId = @StorageTypeId)
                    AND (@TargetId IS NULL OR SL.TargetId = @TargetId)

                    GROUP BY 
                        SL.UnitId,
                        SL.ItemId,
                        IM.ItemCode,
                        IM.ItemName,
                        SL.WarehouseId,
                        SL.StorageTypeId,
                        SL.TargetId,
                        SL.UomId,
                        U.UomName,
                        MM.Description

                    HAVING SUM(SL.ReceivedQty - SL.IssueQty) <> 0
                ";

                var result = await _dbConnection.QueryAsync<StockSummaryDivsionwiseDto>(
                    sql,
                    new
                    {
                        UnitIds = unitIds,
                        ItemId = itemId,
                        WarehouseId = warehouseId,
                        StorageTypeId = storageTypeId,
                        TargetId = targetId
                    });

                return result.ToList();
            }

        public async Task<List<StockSummaryDto>> GetStockSummaryAsync(int? itemId = null, int? warehouseId = null, int? storageTypeId = null, int? targetId = null)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            // Base SQL
            var sql = @"
               SELECT 
                    SL.ItemId,
                    IM.ItemCode,
                    IM.ItemName,

                    SL.WarehouseId,
                    SL.StorageTypeId,
                    SL.TargetId,

                    SL.UomId,
                    U.UomName,

                    SUM(SL.ReceivedQty - SL.IssueQty) AS CurrentStockQty,
                    SUM(SL.ReceivedValue - SL.IssueValue) AS CurrentStockValue,

                    CASE 
                        WHEN SUM(SL.ReceivedQty - SL.IssueQty) <> 0 
                        THEN SUM(SL.ReceivedValue - SL.IssueValue) 
                            / SUM(SL.ReceivedQty - SL.IssueQty)
                        ELSE 0 
                    END AS AvgRate,
                    MM.description as IssueRuleName

                FROM Inventory.StockLedger SL
                INNER JOIN Inventory.ItemMaster IM 
                    ON IM.Id = SL.ItemId
                INNER JOIN Inventory.UOM U 
                    ON U.Id = SL.UomId

                 INNER JOIN Inventory.MiscMaster MM
				   ON IM.IssueRuleId=MM.Id

                WHERE SL.UnitId = @UnitId
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
                GROUP BY 
                    SL.ItemId,
                    IM.ItemCode,
                    IM.ItemName,
                    SL.WarehouseId,
                    SL.StorageTypeId,
                    SL.TargetId,
                    SL.UomId,
                    U.UomName,
                    MM.description
                HAVING SUM(SL.ReceivedQty - SL.IssueQty) <> 0;
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
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            // Base SQL
            var sql = @"
                
                  SELECT 
                    SSL.ItemId,
                    IM.ItemCode,
                    IM.ItemName,

                    SSL.DepartmentId,
                    SSL.WarehouseId,
                    SSL.StorageTypeId,
                    SSL.TargetId,

                    SSL.UomId,
                    U.UomName,

                    SUM(SSL.ReceivedQty - SSL.IssueQty) AS CurrentStockQty,
                    SUM(SSL.ReceivedValue - SSL.IssueValue) AS CurrentStockValue,

                    CASE 
                        WHEN SUM(SSL.ReceivedQty - SSL.IssueQty) <> 0 
                        THEN SUM(SSL.ReceivedValue - SSL.IssueValue) 
                            / SUM(SSL.ReceivedQty - SSL.IssueQty)
                        ELSE 0 
                    END AS AvgRate

                FROM Inventory.SubStoreStockLedger SSL
                INNER JOIN Inventory.ItemMaster IM 
                    ON IM.Id = SSL.ItemId
                INNER JOIN Inventory.UOM U 
                    ON U.Id = SSL.UomId

                WHERE SSL.UnitId = @UnitId
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
               GROUP BY 
                    SSL.ItemId,
                    IM.ItemCode,
                    IM.ItemName,
                    SSL.DepartmentId,
                    SSL.WarehouseId,
                    SSL.StorageTypeId,
                    SSL.TargetId,
                    SSL.UomId,
                    U.UomName
                HAVING SUM(SSL.ReceivedQty - SSL.IssueQty) <> 0;
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