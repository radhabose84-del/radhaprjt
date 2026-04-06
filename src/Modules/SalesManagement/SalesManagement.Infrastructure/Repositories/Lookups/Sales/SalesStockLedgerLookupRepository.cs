using System.Data;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesStockLedgerLookupRepository : ISalesStockLedgerService
{
    private readonly IDbConnection _dbConnection;
    private readonly IItemLookup _itemLookup;
    private readonly ILotMasterLookup _lotMasterLookup;
    private readonly IPackTypeLookup _packTypeLookup;

    public SalesStockLedgerLookupRepository(
        IDbConnection dbConnection,
        IItemLookup itemLookup,
        ILotMasterLookup lotMasterLookup,
        IPackTypeLookup packTypeLookup)
    {
        _dbConnection = dbConnection;
        _itemLookup = itemLookup;
        _lotMasterLookup = lotMasterLookup;
        _packTypeLookup = packTypeLookup;
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
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                sl.ItemId,
                COUNT(sl.PackNo)   AS TotalBags,
                SUM(sl.TotalValue) AS TotalNetWeight
            FROM Sales.StockLedger sl
            INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
            WHERE mm.Description   = 'Packed'
              AND YEAR(sl.DocDate) = @ProductionYear
              AND sl.UnitId        = @UnitId
            GROUP BY sl.ItemId
            ORDER BY sl.ItemId;";

        var items = (await _dbConnection.QueryAsync<StockItemSummaryDto>(sql,
            new { ProductionYear = productionYear, UnitId = unitId })).ToList();

        if (items.Count > 0)
        {
            var itemIds = items.Select(x => x.ItemId).Distinct();
            var itemLookup = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemDict = itemLookup.ToDictionary(x => x.Id, x => x.ItemName);
            foreach (var item in items)
                item.ItemName = itemDict.GetValueOrDefault(item.ItemId);
        }

        return items.AsReadOnly();
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

    public async Task<StockPackSourceDto?> GetPackSourceInfoAsync(
        int startPackNo, int endPackNo, int productionYear, int unitId,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TOP 1
                LotId, WarehouseId, BinId,
                TotalValue                AS OldNetWeightPerPack,
                COUNT(*) OVER ()          AS OldTotalBags,
                SUM(TotalValue) OVER ()   AS OldNetWeight
            FROM Sales.StockLedger
            WHERE PackNo BETWEEN @StartPackNo AND @EndPackNo
              AND YEAR(DocDate) = @ProductionYear
              AND UnitId = @UnitId
            ORDER BY PackNo;";

        return await _dbConnection.QueryFirstOrDefaultAsync<StockPackSourceDto>(sql,
            new { StartPackNo = startPackNo, EndPackNo = endPackNo, ProductionYear = productionYear, UnitId = unitId });
    }

    public async Task<IReadOnlyList<StockPackSummaryDto>> GetPacksByItemAndLotAsync(
        int itemId, int? lotId, int productionYear, int unitId,
        CancellationToken ct = default)
    {
        const string sql = @"
            SELECT
                sl.LotId,
                sl.PackTypeId,
                COUNT(sl.PackNo)   AS TotalBags,
                SUM(sl.TotalValue) AS NetWeight
            FROM Sales.StockLedger sl
            INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
            WHERE mm.Description   = 'Packed'
              AND sl.ItemId        = @ItemId
              AND YEAR(sl.DocDate) = @ProductionYear
              AND sl.UnitId        = @UnitId
              AND (@LotId IS NULL OR sl.LotId = @LotId)
            GROUP BY sl.LotId, sl.PackTypeId
            ORDER BY sl.LotId, sl.PackTypeId;";

        var dp = new DynamicParameters();
        dp.Add("ItemId", itemId);
        dp.Add("LotId", lotId);
        dp.Add("ProductionYear", productionYear);
        dp.Add("UnitId", unitId);

        var items = (await _dbConnection.QueryAsync<StockPackSummaryDto>(sql, dp)).ToList();

        if (items.Count > 0)
        {
            var lotIds = items.Select(x => x.LotId).Distinct();
            var packTypeIds = items.Select(x => x.PackTypeId).Distinct();

            var lotLookup = await _lotMasterLookup.GetByIdsAsync(lotIds, ct);
            var packTypeLookup = await _packTypeLookup.GetByIdsAsync(packTypeIds, ct);

            var lotDict = lotLookup.ToDictionary(x => x.Id);
            var packTypeDict = packTypeLookup.ToDictionary(x => x.Id);

            foreach (var item in items)
            {
                if (lotDict.TryGetValue(item.LotId, out var lot))
                {
                    item.LotCode = lot.LotCode;
                    item.BatchNumber = lot.BatchNumber;
                }
                if (packTypeDict.TryGetValue(item.PackTypeId, out var pt))
                {
                    item.PackTypeCode = pt.PackTypeCode;
                    item.PackTypeName = pt.PackTypeName;
                }
            }
        }

        return items.AsReadOnly();
    }
}
