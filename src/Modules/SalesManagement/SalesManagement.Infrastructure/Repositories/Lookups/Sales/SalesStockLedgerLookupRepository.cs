using System.Data;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesStockLedgerLookupRepository : ISalesStockLedgerService
{
    private readonly IDbConnection _dbConnection;

    public SalesStockLedgerLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> InsertAsync(List<SalesStockLedgerDto> entries, CancellationToken ct = default)
    {
        if (entries == null || entries.Count == 0)
            return false;

        const string sql = @"
            INSERT INTO Sales.StockLedger
                (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId, PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId, TypeId)
            VALUES
                (@UnitId, @DocType, @DocNo, @DetailDocNo, @DocDate, @ItemId, @LotId, @PackNo, @PackTypeId, @WarehouseId, @BinId, @TotalQty, @TotalValue, @StatusId, @TypeId)";

        await _dbConnection.ExecuteAsync(sql, entries);
        return true;
    }

    public async Task<bool> DeleteByDocAsync(string docType, int docNo, int productionYear, int unitId, CancellationToken ct = default)
    {
        const string sql = @"
            DELETE FROM Sales.StockLedger
            WHERE DocType = @DocType
              AND DocNo   = @DocNo
              AND YEAR(DocDate) = @ProductionYear
              AND UnitId  = @UnitId";

        var affected = await _dbConnection.ExecuteAsync(sql,
            new { DocType = docType, DocNo = docNo, ProductionYear = productionYear, UnitId = unitId });
        return affected > 0;
    }

    public async Task<bool> UpdateStatusByPackRangeAsync(
        string docType,
        int startPackNo, int endPackNo,
        int currentStatusId, int newStatusId,
        int productionYear, int unitId,
        CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE Sales.StockLedger
            SET StatusId = @NewStatusId
            WHERE DocType  = @DocType
              AND PackNo   BETWEEN @StartPackNo AND @EndPackNo
              AND StatusId = @CurrentStatusId
              AND YEAR(DocDate) = @ProductionYear
              AND UnitId   = @UnitId";

        var affected = await _dbConnection.ExecuteAsync(sql, new
        {
            DocType         = docType,
            StartPackNo     = startPackNo,
            EndPackNo       = endPackNo,
            CurrentStatusId = currentStatusId,
            NewStatusId     = newStatusId,
            ProductionYear  = productionYear,
            UnitId          = unitId
        });
        return affected > 0;
    }

    public async Task<IReadOnlyList<int>> GetPackedPackNosAsync(
        int startPackNo, int endPackNo,
        int productionYear, int unitId,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT DISTINCT sl.PackNo
            FROM Sales.StockLedger sl
            INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
            WHERE sl.PackNo BETWEEN @StartPackNo AND @EndPackNo
              AND mm.Description = 'Packed'
              AND YEAR(sl.DocDate) = @ProductionYear
              AND sl.UnitId = @UnitId
            ORDER BY sl.PackNo;";

        var result = await _dbConnection.QueryAsync<int>(sql,
            new { StartPackNo = startPackNo, EndPackNo = endPackNo, ProductionYear = productionYear, UnitId = unitId });

        return result.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<StockItemSummaryDto>> GetStockItemsAsync(
        int productionYear, int unitId,
        int? packTypeId = null,
        CancellationToken ct = default)
    {
        string sql;
        object parameters;

        if (packTypeId == null)
        {
            sql = @"
                SELECT
                    sl.ItemId,
                    0 AS PackTypeId,
                    COUNT(sl.PackNo) AS TotalPackedBags,
                    SUM(sl.TotalValue) AS TotalNetWeight
                FROM Sales.StockLedger sl
                INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE mm.Description = 'Packed'
                  AND YEAR(sl.DocDate) = @ProductionYear
                  AND sl.UnitId = @UnitId
                GROUP BY sl.ItemId
                ORDER BY sl.ItemId;";
            parameters = new { ProductionYear = productionYear, UnitId = unitId };
        }
        else
        {
            sql = @"
                SELECT
                    sl.ItemId,
                    sl.PackTypeId,
                    COUNT(sl.PackNo) AS TotalPackedBags,
                    SUM(sl.TotalValue) AS TotalNetWeight
                FROM Sales.StockLedger sl
                INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE mm.Description = 'Packed'
                  AND YEAR(sl.DocDate) = @ProductionYear
                  AND sl.UnitId = @UnitId
                  AND sl.PackTypeId = @PackTypeId
                GROUP BY sl.ItemId, sl.PackTypeId
                ORDER BY sl.ItemId;";
            parameters = new { ProductionYear = productionYear, UnitId = unitId, PackTypeId = packTypeId };
        }

        var result = await _dbConnection.QueryAsync<StockItemSummaryDto>(sql, parameters);
        return result.ToList().AsReadOnly();
    }

    public async Task<int> GetLastPackNoByYearAsync(int productionYear, int unitId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT ISNULL(MAX(sl.PackNo), 0)
            FROM Sales.StockLedger sl
            WHERE YEAR(sl.DocDate) = @ProductionYear
              AND sl.UnitId = @UnitId";

        return await _dbConnection.ExecuteScalarAsync<int>(sql,
            new { ProductionYear = productionYear, UnitId = unitId });
    }

    public async Task<int> GetLotIdByPackRangeAsync(int startPackNo, int endPackNo, int productionYear, int unitId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TOP 1 LotId
            FROM Sales.StockLedger
            WHERE PackNo BETWEEN @StartPackNo AND @EndPackNo
              AND YEAR(DocDate) = @ProductionYear
              AND UnitId = @UnitId
              AND LotId != 0
            ORDER BY PackNo;";

        return await _dbConnection.ExecuteScalarAsync<int>(sql,
            new { StartPackNo = startPackNo, EndPackNo = endPackNo, ProductionYear = productionYear, UnitId = unitId });
    }

    public async Task<IReadOnlyList<StockPackByItemLotDto>> GetPacksByItemAndLotAsync(
        int itemId, int lotId, int productionYear, int unitId,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                sl.PackNo,
                sl.PackTypeId,
                sl.WarehouseId,
                sl.BinId,
                sl.TotalValue AS NetWeight,
                sl.LotId,
                sl.ItemId
            FROM Sales.StockLedger sl
            INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
            WHERE mm.Description = 'Packed'
              AND sl.ItemId        = @ItemId
              AND sl.LotId         = @LotId
              AND YEAR(sl.DocDate) = @ProductionYear
              AND sl.UnitId        = @UnitId
            ORDER BY sl.PackNo;";

        var result = await _dbConnection.QueryAsync<StockPackByItemLotDto>(sql,
            new { ItemId = itemId, LotId = lotId, ProductionYear = productionYear, UnitId = unitId });
        return result.ToList().AsReadOnly();
    }
}
