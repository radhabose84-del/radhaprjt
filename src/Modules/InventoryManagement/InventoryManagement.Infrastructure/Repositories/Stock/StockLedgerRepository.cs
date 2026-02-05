using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Interfaces.IStock;
using InventoryManagement.Domain.Entities.Stock;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.Infrastructure.Repositories.Stock
{
    public class StockLedgerRepository : IStockLedgerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StockLedgerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task InsertStockLedgerDataAsync(List<StockLedger> stockLedgers, CancellationToken cancellationToken = default)
        {
            if (stockLedgers == null || stockLedgers.Count == 0)
                return;

            await _dbContext.StockLedger.AddRangeAsync(
                stockLedgers, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task InsertSubStoreStockLedgerDataAsync(List<SubStoreStockLedger> subStoreStockLedgers, CancellationToken cancellationToken = default)
        {
            if (subStoreStockLedgers == null || subStoreStockLedgers.Count == 0)
                return;

            await _dbContext.SubStoreStockLedger.AddRangeAsync(
                subStoreStockLedgers, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}