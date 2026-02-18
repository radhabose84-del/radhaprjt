using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Inventory;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class StockLedgerLookupRepository : IStockLedgerLookup
    {
        private readonly ApplicationDbContext _db;

        public StockLedgerLookupRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> InsertStockLedgerAsync(
            List<StockLedgerDto> stockLedgers,
            CancellationToken ct = default)
        {
            if (stockLedgers == null || stockLedgers.Count == 0)
                return true;

            var entities = stockLedgers.Select(x => new StockLedger
            {
                UnitId = x.UnitId,
                DocType = x.DocType,
                DocNo = x.DocNo,
                DocSlNo = x.DocSlNo,
                DocDate = x.DocDate,
                ItemId = x.ItemId,
                UomId = x.UomId,
                WarehouseId = x.WarehouseId,
                StorageTypeId = x.StorageTypeId,
                TargetId = x.TargetId,
                ReceivedQty = x.ReceivedQty,
                ReceivedValue = x.ReceivedValue,
                IssueQty = x.IssueQty,
                IssueValue = x.IssueValue
            }).ToList();

            await _db.StockLedger.AddRangeAsync(entities, ct);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> InsertSubStoreStockLedgerAsync(
            List<SubStoreStockLedgerDto> subStoreStockLedgers,
            CancellationToken ct = default)
        {
            if (subStoreStockLedgers == null || subStoreStockLedgers.Count == 0)
                return true;

            var entities = subStoreStockLedgers.Select(x => new SubStoreStockLedger
            {
                UnitId = x.UnitId,
                DocType = x.DocType,
                DocNo = x.DocNo,
                DocSlNo = x.DocSlNo,
                DocDate = x.DocDate,
                DepartmentId = x.DepartmentId,
                ItemId = x.ItemId,
                UomId = x.UomId,
                WarehouseId = x.WarehouseId,
                StorageTypeId = x.StorageTypeId,
                TargetId = x.TargetId,
                ReceivedQty = x.ReceivedQty,
                ReceivedValue = x.ReceivedValue,
                IssueQty = x.IssueQty,
                IssueValue = x.IssueValue
            }).ToList();

            await _db.SubStoreStockLedger.AddRangeAsync(entities, ct);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
