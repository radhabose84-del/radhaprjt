using System.Data;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesStockLedgerLookupRepository : ISalesStockLedgerLookup
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
                (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId, PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId)
            VALUES
                (@UnitId, @DocType, @DocNo, @DetailDocNo, @DocDate, @ItemId, @LotId, @PackNo, @PackTypeId, @WarehouseId, @BinId, @TotalQty, @TotalValue, @StatusId)";

        await _dbConnection.ExecuteAsync(sql, entries);
        return true;
    }

    public async Task<bool> DeleteByDocAsync(string docType, int docNo, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM Sales.StockLedger WHERE DocType = @DocType AND DocNo = @DocNo";
        var affected = await _dbConnection.ExecuteAsync(sql, new { DocType = docType, DocNo = docNo });
        return affected > 0;
    }
}
