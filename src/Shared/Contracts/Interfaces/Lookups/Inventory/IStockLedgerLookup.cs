using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IStockLedgerLookup
    {
        Task<bool> InsertStockLedgerAsync(
            List<StockLedgerDto> stockLedgers,
            CancellationToken ct = default);

        Task<bool> InsertSubStoreStockLedgerAsync(
            List<SubStoreStockLedgerDto> subStoreStockLedgers,
            CancellationToken ct = default);
    }
}
