using System.Data;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class StockLedgerLookupRepository : IStockLedgerLookup
    {
        private readonly IDbConnection _dbConnection;

        public StockLedgerLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> InsertStockLedgerAsync(
            List<StockLedgerDto> stockLedgers,
            IDbTransaction? transaction = null,
            CancellationToken ct = default)
        {
            if (stockLedgers == null || stockLedgers.Count == 0)
                return true;

            const string sql = @"
                INSERT INTO [Inventory].[StockLedger]
                    (UnitId, DocType, DocNo, DocSlNo, DocDate, ItemId, UomId,
                     WarehouseId, StorageTypeId, TargetId, ReceivedQty, ReceivedValue, IssueQty, IssueValue)
                VALUES
                    (@UnitId, @DocType, @DocNo, @DocSlNo, @DocDate, @ItemId, @UomId,
                     @WarehouseId, @StorageTypeId, @TargetId, @ReceivedQty, @ReceivedValue, @IssueQty, @IssueValue)";

            // Use transaction's connection when shared transaction is provided, otherwise use injected connection
            var conn = transaction?.Connection ?? _dbConnection;
            await conn.ExecuteAsync(sql, stockLedgers, transaction: transaction);
            return true;
        }

        public async Task<bool> InsertSubStoreStockLedgerAsync(
            List<SubStoreStockLedgerDto> subStoreStockLedgers,
            IDbTransaction? transaction = null,
            CancellationToken ct = default)
        {
            if (subStoreStockLedgers == null || subStoreStockLedgers.Count == 0)
                return true;

            const string sql = @"
                INSERT INTO [Inventory].[SubStoreStockLedger]
                    (UnitId, DocType, DocNo, DocSlNo, DocDate, DepartmentId, ItemId, UomId,
                     WarehouseId, StorageTypeId, TargetId, ReceivedQty, ReceivedValue, IssueQty, IssueValue)
                VALUES
                    (@UnitId, @DocType, @DocNo, @DocSlNo, @DocDate, @DepartmentId, @ItemId, @UomId,
                     @WarehouseId, @StorageTypeId, @TargetId, @ReceivedQty, @ReceivedValue, @IssueQty, @IssueValue)";

            var conn = transaction?.Connection ?? _dbConnection;
            await conn.ExecuteAsync(sql, subStoreStockLedgers, transaction: transaction);
            return true;
        }
    }
}
