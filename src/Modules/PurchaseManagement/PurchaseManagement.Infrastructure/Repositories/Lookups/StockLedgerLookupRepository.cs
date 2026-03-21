// ============================================================
// OLD CODE — StockLedger was owned by PurchaseManagement (wrong ownership)
// Moved to InventoryManagement.Infrastructure/Repositories/Lookups/StockLedgerLookupRepository.cs
// DI registration removed from PurchaseManagement.Infrastructure/DependencyInjection.cs
// ============================================================
// using Contracts.Dtos.Stock;
// using Contracts.Interfaces.Lookups.Inventory;
// using PurchaseManagement.Domain.Entities.GRN.StockLedger;
// using PurchaseManagement.Domain.Entities.MRS;
// using PurchaseManagement.Infrastructure.Data;
//
// namespace PurchaseManagement.Infrastructure.Repositories.Lookups
// {
//     internal sealed class StockLedgerLookupRepository : IStockLedgerLookup
//     {
//         private readonly ApplicationDbContext _db;
//         public StockLedgerLookupRepository(ApplicationDbContext db) => _db = db;
//
//         public async Task<bool> InsertStockLedgerAsync(List<StockLedgerDto> stockLedgers, CancellationToken ct = default)
//         {
//             var entities = stockLedgers.Select(x => new StockLedger { ... }).ToList();
//             await _db.StockLedger.AddRangeAsync(entities, ct);
//             await _db.SaveChangesAsync(ct);
//             return true;
//         }
//
//         public async Task<bool> InsertSubStoreStockLedgerAsync(List<SubStoreStockLedgerDto> subStoreStockLedgers, CancellationToken ct = default)
//         {
//             var entities = subStoreStockLedgers.Select(x => new SubStoreStockLedger { ... }).ToList();
//             await _db.SubStoreStockLedger.AddRangeAsync(entities, ct);
//             await _db.SaveChangesAsync(ct);
//             return true;
//         }
//     }
// }
