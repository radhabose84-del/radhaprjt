using InventoryManagement.Domain.Entities.Stock;

namespace InventoryManagement.Application.Common.Interfaces.IStock
{
    public interface IStockLedgerRepository
    {
        Task InsertStockLedgerDataAsync(
        List<StockLedger> stockLedgers,
        CancellationToken cancellationToken = default);
            
        Task InsertSubStoreStockLedgerDataAsync(
            List<SubStoreStockLedger> subStoreStockLedgers,
            CancellationToken cancellationToken = default);
    }
}