using System.Data;
using System.Text;
using Contracts.Interfaces;
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
        private readonly IIPAddressService _ipAddressService;

        public StockLedgerReportRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection    = dbConnection;
            _unitLookup      = unitLookup;
            _itemLookup      = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup       = binLookup;
            _packTypeLookup  = packTypeLookup;
            _lotMasterLookup = lotMasterLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<StockLedgerReportDto>, int)> GetReportAsync(
            int pageNumber,
            int pageSize,
            int? itemId,
            int? lotId,
            int? warehouseId,
            int? binId,
            int? statusId,
            int? packNo,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            int? productionYear)
        {
            var unitId = _ipAddressService.GetUnitId();
            var effectiveYear = productionYear ?? DateTime.UtcNow.Year;
            var where = new StringBuilder("YEAR(sl.DocDate) = @ProductionYear");

            if (unitId.HasValue)      where.Append(" AND sl.UnitId = @UnitId");
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
                    sl.Id, sl.UnitId, sl.SourceUnitId, sl.DocType, sl.DocNo, sl.DetailDocNo, sl.DocDate,
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
                ProductionYear = effectiveYear,
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
                row.UnitName       = unitDict.TryGetValue(row.UnitId,      out var un) ? un : null;
                row.SourceUnitName = row.SourceUnitId.HasValue && unitDict.TryGetValue(row.SourceUnitId.Value, out var sun) ? sun : null;
                row.ItemName       = itemDict.TryGetValue(row.ItemId,      out var im) ? im : null;
                row.WarehouseName  = warehouseDict.TryGetValue(row.WarehouseId, out var wn) ? wn : null;
                row.BinName        = binDict.TryGetValue(row.BinId,        out var bn) ? bn : null;
                row.PackTypeName   = packTypeDict.TryGetValue(row.PackTypeId, out var pn) ? pn : null;
                row.LotCode        = lotDict.TryGetValue(row.LotId,       out var lc) ? lc : null;
            }

            return (list, total);
        }

        public async Task<List<StockLedgerReportDto>> GetByPackRangeAsync(
            int itemId,
            int packTypeId,
            int startPackNo,
            int endPackNo,
            int productionYear,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT
                    sl.Id, sl.UnitId, sl.SourceUnitId, sl.DocType, sl.DocNo, sl.DetailDocNo, sl.DocDate,
                    sl.ItemId, sl.LotId,
                    sl.PackNo, sl.PackTypeId,
                    sl.WarehouseId, sl.BinId,
                    sl.TotalQty, sl.TotalValue,
                    sl.StatusId,
                    mm.Description AS StatusName
                FROM Sales.StockLedger sl
                INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                                                    AND mtm.MiscTypeCode = 'StockStatus'
                                                    AND mtm.IsDeleted = 0
                WHERE sl.ItemId      = @ItemId
                  AND sl.PackTypeId  = @PackTypeId
                  AND sl.PackNo     BETWEEN @StartPackNo AND @EndPackNo
                  AND YEAR(sl.DocDate) = @ProductionYear
                  AND sl.UnitId      = @UnitId
                  AND mm.Description = 'Packed'
                ORDER BY sl.PackNo ASC;";

            var list = (await _dbConnection.QueryAsync<StockLedgerReportDto>(
                new CommandDefinition(sql, new
                {
                    ItemId      = itemId,
                    PackTypeId  = packTypeId,
                    StartPackNo = startPackNo,
                    EndPackNo   = endPackNo,
                    ProductionYear = productionYear,
                    UnitId      = unitId
                }, cancellationToken: ct))).ToList();

            if (list.Count == 0)
                return list;

            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var itemIds = list.Select(x => x.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var warehouses = await _warehouseLookup.GetAllAsync();
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

            var bins = await _binLookup.GetAllAsync();
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            var packTypeIds = list.Where(x => x.PackTypeId > 0).Select(x => x.PackTypeId).Distinct();
            var packTypes = packTypeIds.Any() ? await _packTypeLookup.GetByIdsAsync(packTypeIds) : [];
            var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

            foreach (var row in list)
            {
                row.UnitName       = unitDict.TryGetValue(row.UnitId,      out var un) ? un : null;
                row.SourceUnitName = row.SourceUnitId.HasValue && unitDict.TryGetValue(row.SourceUnitId.Value, out var sun) ? sun : null;
                row.ItemName       = itemDict.TryGetValue(row.ItemId,      out var im) ? im : null;
                row.WarehouseName  = warehouseDict.TryGetValue(row.WarehouseId, out var wn) ? wn : null;
                row.BinName        = binDict.TryGetValue(row.BinId,        out var bn) ? bn : null;
                row.PackTypeName   = packTypeDict.TryGetValue(row.PackTypeId, out var pn) ? pn : null;
            }

            return list;
        }

        public async Task<List<PackRangeSummaryDto>> GetPackRangeSummaryAsync(
            int productionYear,
            int? itemId,
            int? packTypeId,
            int? startPackNo,
            int? endPackNo,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var where = new StringBuilder(@"YEAR(sl.DocDate) = @ProductionYear
                  AND sl.UnitId = @UnitId
                  AND mm.Description = 'Packed'");

            if (itemId.HasValue)
                where.Append(" AND sl.ItemId = @ItemId");
            if (packTypeId.HasValue)
                where.Append(" AND sl.PackTypeId = @PackTypeId");
            if (startPackNo.HasValue && endPackNo.HasValue)
                where.Append(" AND sl.PackNo BETWEEN @StartPackNo AND @EndPackNo");

            var sql = $@"
                SELECT
                    sl.ItemId, sl.PackTypeId,
                    COUNT(DISTINCT sl.PackNo) AS TotalPacks,
                    MIN(sl.PackNo) AS StartPackNo,
                    MAX(sl.PackNo) AS EndPackNo
                FROM Sales.StockLedger sl
                INNER JOIN Sales.MiscMaster mm ON sl.StatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                    AND mtm.MiscTypeCode = 'StockStatus' AND mtm.IsDeleted = 0
                WHERE {where}
                GROUP BY sl.ItemId, sl.PackTypeId
                ORDER BY sl.ItemId, sl.PackTypeId";

            var list = (await _dbConnection.QueryAsync<PackRangeSummaryDto>(
                new CommandDefinition(sql, new
                {
                    ItemId = itemId,
                    PackTypeId = packTypeId,
                    ProductionYear = productionYear,
                    StartPackNo = startPackNo,
                    EndPackNo = endPackNo,
                    UnitId = unitId
                }, cancellationToken: ct))).ToList();

            if (list.Count == 0)
                return list;

            // Populate ItemName via cross-module lookup
            var itemIds = list.Select(x => x.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var packTypeIds = list.Where(x => x.PackTypeId > 0).Select(x => x.PackTypeId).Distinct();
            Dictionary<int, string?>? packTypeDict = null;
            if (packTypeIds.Any())
            {
                var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
                packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);
            }

            foreach (var row in list)
            {
                row.ItemName = itemDict.TryGetValue(row.ItemId, out var im) ? im : null;
                if (packTypeDict != null)
                    row.PackTypeName = packTypeDict.TryGetValue(row.PackTypeId, out var pn) ? pn : null;
            }

            return list;
        }
    }
}
