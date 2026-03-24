using System.Data;
using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IStockLedgerLookup
    {
        Task<bool> InsertStockLedgerAsync(
            List<StockLedgerDto> stockLedgers,
            IDbTransaction? transaction = null,
            CancellationToken ct = default);

        Task<bool> InsertSubStoreStockLedgerAsync(
            List<SubStoreStockLedgerDto> subStoreStockLedgers,
            IDbTransaction? transaction = null,
            CancellationToken ct = default);
    }
}
