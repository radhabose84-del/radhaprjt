using System.Data;
using System.Text;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;

namespace SalesManagement.Infrastructure.Repositories.Reports.StockLedger
{
    internal sealed class StockLedgerReportRepository : IStockLedgerReportRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly ILotMasterLookup _lotMasterLookup;

        public StockLedgerReportRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _packTypeLookup = packTypeLookup;
            _lotMasterLookup = lotMasterLookup;
        }

        public async Task<(List<StockLedgerReportDto>, int)> GetReportAsync(
            int unitId,
            int pageNumber,
            int pageSize,
            int? itemId,
            int? lotId,
            int? warehouseId,
            int? binId,
            int? statusId,
            int? packNo,
            DateOnly? dateFrom,
            DateOnly? dateTo)
        {
            var where = new StringBuilder("sl.UnitId = @UnitId");

            if (itemId.HasValue)      where.Append(" AND sl.ItemId = @ItemId");
            if (lotId.HasValue)       where.Append(" AND sl.LotId = @LotId");
            if (warehouseId.HasValue) where.Append(" AND sl.WarehouseId = @WarehouseId");
            if (binId.HasValue)       where.Append(" AND sl.BinId = @BinId");
            if (statusId.HasValue)    where.Append(" AND sl.StatusId = @StatusId");
            if (packNo.HasValue)      where.Append(" AND sl.PackNo = @PackNo");
            if (dateFrom.HasValue)    where.Append(" AND sl.DocDate >= @DateFrom");
            if (dateTo.HasValue)      where.Append(" AND sl.DocDate <= @DateTo");

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.StockLedger sl
                WHERE {where};

                SELECT
                    sl.Id, sl.UnitId, sl.DocType, sl.DocNo, sl.DetailDocNo, sl.DocDate,
                    sl.ItemId, sl.LotId,
                    sl.PackNo, sl.PackTypeId,
                    sl.WarehouseId, sl.BinId,
                    sl.TotalQty, sl.TotalValue,
                    sl.StatusId,
                    mm.Description AS StatusName
                FROM Sales.StockLedger sl
                LEFT JOIN Sales.MiscMaster mm ON sl.StatusId     = mm.Id  AND mm.IsDeleted  = 0
                LEFT JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                                                   AND mtm.MiscTypeCode = 'StockStatus'
                                                   AND mtm.IsDeleted = 0
                WHERE {where}
                ORDER BY sl.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                UnitId      = unitId,
                ItemId      = itemId,
                LotId       = lotId,
                WarehouseId = warehouseId,
                BinId       = binId,
                StatusId    = statusId,
                PackNo      = packNo,
                DateFrom    = dateFrom.HasValue ? (DateTime?)dateFrom.Value.ToDateTime(TimeOnly.MinValue) : null,
                DateTo      = dateTo.HasValue   ? (DateTime?)dateTo.Value.ToDateTime(TimeOnly.MinValue)   : null,
                Offset      = (pageNumber - 1) * pageSize,
                PageSize    = pageSize
            };

            var multi  = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list   = (await multi.ReadAsync<StockLedgerReportDto>()).ToList();
            var total  = await multi.ReadFirstAsync<int>();

            if (list.Count == 0)
                return (list, total);

            // ── Cross-module lookups ──────────────────────────────────────
            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var itemIds = list.Select(x => x.ItemId).Distinct();
            var items   = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var warehouses = await _warehouseLookup.GetAllAsync();
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

            var bins = await _binLookup.GetAllAsync();
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            var packTypeIds = list.Where(x => x.PackTypeId > 0).Select(x => x.PackTypeId).Distinct();
            var packTypes = packTypeIds.Any() ? await _packTypeLookup.GetByIdsAsync(packTypeIds) : [];
            var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

            var lotIds = list.Where(x => x.LotId > 0).Select(x => x.LotId).Distinct();
            var lots = lotIds.Any() ? await _lotMasterLookup.GetByIdsAsync(lotIds) : [];
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

            foreach (var row in list)
            {
                row.UnitName      = unitDict.TryGetValue(row.UnitId,      out var un) ? un : null;
                row.ItemName      = itemDict.TryGetValue(row.ItemId,      out var im) ? im : null;
                row.WarehouseName = warehouseDict.TryGetValue(row.WarehouseId, out var wn) ? wn : null;
                row.BinName       = binDict.TryGetValue(row.BinId,        out var bn) ? bn : null;
                row.PackTypeName  = packTypeDict.TryGetValue(row.PackTypeId, out var pn) ? pn : null;
                row.LotCode       = lotDict.TryGetValue(row.LotId,       out var lc) ? lc : null;
            }

            return (list, total);
        }
    }
}
