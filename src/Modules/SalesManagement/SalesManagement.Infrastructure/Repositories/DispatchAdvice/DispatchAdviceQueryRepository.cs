using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Infrastructure.Repositories.DispatchAdvice
{
    public class DispatchAdviceQueryRepository : IDispatchAdviceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly ILotMasterLookup _lotMasterLookup;
        private readonly IFreightMasterLookup _freightMasterLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public DispatchAdviceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IIPAddressService ipAddressService,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup,
            IFreightMasterLookup freightMasterLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _ipAddressService = ipAddressService;
            _packTypeLookup = packTypeLookup;
            _lotMasterLookup = lotMasterLookup;
            _freightMasterLookup = freightMasterLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
        }

        public async Task<(List<DispatchAdviceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.DispatchNo LIKE @Search OR h.VehicleNo LIKE @Search OR h.DriverName LIKE @Search OR h.LRNo LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.DispatchAdviceHeader h
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.DispatchNo, h.DispatchDate,
                    h.StatusId,
                    mm.Description AS StatusName,
                    h.SalesOrderId,
                    so.SalesOrderNo,
                    h.PartyId,
                    h.TotOrderQty, h.TotDispatchedQty, h.TotPendingQty,
                    h.DispatchAddressId,
                    da.DispatchAddressName,
                    h.DispatchTypeId,
                    dt.Description AS DispatchTypeName,
                    h.FreightId,
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.UnitId, h.InvFlg,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DispatchTypeId = dt.Id AND dt.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<DispatchAdviceHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Populate cross-module: PartyName
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                // Populate cross-module: TransporterName (also from PartyLookup)
                var transporterIds = list.Where(x => x.TransporterId.HasValue).Select(x => x.TransporterId!.Value).Distinct();
                var transporters = transporterIds.Any()
                    ? await _partyLookup.GetByIdsAsync(transporterIds)
                    : [];
                var transporterDict = transporters.ToDictionary(p => p.Id, p => p.PartyName);

                // Populate cross-module: Freight details (FreightModeName, RateMethodName, Rate)
                var freightIds = list.Select(x => x.FreightId).Distinct().ToList();
                var allFreights = await _freightMasterLookup.GetAllFreightMasterAsync();
                var freightDict = allFreights.Where(f => freightIds.Contains(f.Id)).ToDictionary(f => f.Id);

                foreach (var item in list)
                {
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;

                    if (freightDict.TryGetValue(item.FreightId, out var freightDto))
                    {
                        item.FreightModeName = freightDto.FreightModeName;
                        item.RateMethodName = freightDto.RateMethodName;
                        item.FreightRate = freightDto.Rate;
                    }

                    if (item.TransporterId.HasValue)
                        item.TransporterName = transporterDict.TryGetValue(item.TransporterId.Value, out var tName) ? tName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<DispatchAdviceHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.DispatchNo, h.DispatchDate,
                    h.StatusId,
                    mm.Description AS StatusName,
                    h.SalesOrderId,
                    so.SalesOrderNo,
                    h.PartyId,
                    h.TotOrderQty, h.TotDispatchedQty, h.TotPendingQty,
                    h.DispatchAddressId,
                    da.DispatchAddressName,
                    h.DispatchTypeId,
                    dt.Description AS DispatchTypeName,
                    h.FreightId,
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.UnitId, h.InvFlg,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DispatchTypeId = dt.Id AND dt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAdviceHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Fetch detail rows with same-module JOINs
            const string detailSql = @"
                SELECT d.Id, d.DispatchAdviceHeaderId,
                    d.SalesOrderDetailId,
                    d.ItemId,
                    d.LotId,
                    d.StartPackNo, d.EndPackNo, d.DispatchQty,
                    d.PackTypeId,
                    sod.HSNId,
                    sod.ExMillRate,
                    sod.TaxableAmount,
                    sod.TaxPercentage,
                    sod.TaxAmount,
                    sod.TCSPercentage,
                    sod.TCSAmount,
                    sod.NetAmount,
                    sod.BagWeight
                FROM Sales.DispatchAdviceDetail d
                LEFT JOIN Sales.SalesOrderDetail sod ON d.SalesOrderDetailId = sod.Id
                WHERE d.DispatchAdviceHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<DispatchAdviceDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module: PartyName
            var party = await _partyLookup.GetByIdAsync(header.PartyId);
            header.PartyName = party?.PartyName;

            // Populate cross-module: Freight details (FreightModeName, RateMethodName, Rate)
            var freight = await _freightMasterLookup.GetByIdAsync(header.FreightId);
            header.FreightModeName = freight?.FreightModeName;
            header.RateMethodName = freight?.RateMethodName;
            header.FreightRate = freight?.Rate;

            // Populate cross-module: TransporterName
            if (header.TransporterId.HasValue)
            {
                var transporter = await _partyLookup.GetByIdAsync(header.TransporterId.Value);
                header.TransporterName = transporter?.PartyName;
            }

            // Populate cross-module detail lookups: ItemName
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                // Populate cross-module: HSNCode
                var hsnIds = details.Where(d => d.HSNId.HasValue).Select(d => d.HSNId!.Value).Distinct();
                var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                var hsnDict = hsnList.ToDictionary(h => h.Id, h => h.HSNCode);

                var lotIds = details.Where(d => d.LotId > 0).Select(d => d.LotId).Distinct();
                var lotList = lotIds.Any() ? await _lotMasterLookup.GetByIdsAsync(lotIds) : [];
                var lotDict = lotList.ToDictionary(l => l.Id, l => l.LotCode);

                var packTypeIds = details.Where(d => d.PackTypeId > 0).Select(d => d.PackTypeId).Distinct();
                var packTypeList = packTypeIds.Any() ? await _packTypeLookup.GetByIdsAsync(packTypeIds) : [];
                var packTypeDict = packTypeList.ToDictionary(p => p.Id, p => p.PackTypeName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;

                    if (detail.HSNId.HasValue)
                        detail.HSNCode = hsnDict.TryGetValue(detail.HSNId.Value, out var hCode) ? hCode : null;

                    if (detail.LotId > 0)
                        detail.LotCode = lotDict.TryGetValue(detail.LotId, out var lCode) ? lCode : null;

                    if (detail.PackTypeId > 0)
                        detail.PackTypeName = packTypeDict.TryGetValue(detail.PackTypeId, out var pName) ? pName : null;
                }
            }

            header.Details = details;
            return header;
        }

        public async Task<bool> SalesOrderExistsAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrderId });
            return count > 0;
        }

        public async Task<bool> HasPendingAmendmentAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderAmendmentHeader ah
                    INNER JOIN (
                        SELECT SalesOrderHeaderId, MAX(RevisionNumber) AS MaxRevision
                        FROM Sales.SalesOrderAmendmentHeader
                        WHERE IsDeleted = 0
                        GROUP BY SalesOrderHeaderId
                    ) latest ON ah.SalesOrderHeaderId = latest.SalesOrderHeaderId
                              AND ah.RevisionNumber = latest.MaxRevision
                    INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE ah.SalesOrderHeaderId = @Id AND ah.IsDeleted = 0
                      AND LOWER(mm.Code) = LOWER('Pending')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> DispatchAddressExistsAsync(int dispatchAddressId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dispatchAddressId });
            return count > 0;
        }

        public async Task<int> GetSalesOrderUnitIdAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT OrderUnitId
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrderId });
        }

        public async Task<List<DispatchAdviceStockDto>> GetStockAsync(int itemId, int lotId, int statusId)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT Sum(S.TotalQty) AS Qty, Sum(S.TotalValue) AS Value,
                    S.PackTypeId
                FROM Sales.StockLedger S
                WHERE S.UnitId = @UnitId AND S.ItemId = @ItemId AND S.StatusId = @StatusId AND S.LotId = @LotId
                GROUP BY S.PackTypeId";

            var result = (await _dbConnection.QueryAsync<DispatchAdviceStockDto>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId })).ToList();

            // Populate PackType details via lookup
            var packTypeIds = result.Select(r => r.PackTypeId).Distinct();
            if (packTypeIds.Any())
            {
                var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
                var ptDict = packTypes.ToDictionary(p => p.Id);
                foreach (var item in result)
                {
                    if (ptDict.TryGetValue(item.PackTypeId, out var pt))
                    {
                        item.PackTypeCode = pt.PackTypeCode;
                        item.PackTypeName = pt.PackTypeName;
                        item.NetWeight = pt.NetWeight;
                        item.TareWeight = pt.TareWeight;
                        item.GrossWeight = pt.GrossWeight;
                        item.ConesPerBag = pt.ConesPerBag;
                    }
                }
            }

            return result;
        }


        public async Task<List<int>> GetAvailablePackNosAsync(int itemId, int lotId, int statusId, int startPackNo, int endPackNo, int packTypeId)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT DISTINCT sl.PackNo
                FROM Sales.StockLedger sl
                WHERE sl.UnitId     = @UnitId
                  AND sl.ItemId     = @ItemId
                  AND sl.LotId      = @LotId
                  AND sl.PackTypeId = @PackTypeId
                  AND sl.StatusId   = @StatusId
                  AND sl.PackNo    BETWEEN @StartPackNo AND @EndPackNo
                ORDER BY sl.PackNo";

            var result = await _dbConnection.QueryAsync<int>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId, StartPackNo = startPackNo, EndPackNo = endPackNo, PackTypeId = packTypeId });
            return result.ToList();
        }

        public async Task<List<DispatchAdvicePackRangeDto>> GetPackRangeAsync(int itemId, int lotId, int packTypeId, int statusId, int range, string? orderType)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // FIFO (default) → DocDate, PackNo ASC ; LIFO → DocDate, PackNo DESC
            var isLifo = string.Equals(orderType, "LIFO", StringComparison.OrdinalIgnoreCase);
            var direction = isLifo ? "DESC" : "ASC";

            var sql = $@"
                SELECT S.PackNo, S.ItemId, S.LotId, S.PackTypeId
                FROM Sales.StockLedger S
                WHERE S.UnitId = @UnitId AND S.ItemId = @ItemId AND S.StatusId = @StatusId
                    AND S.LotId = @LotId AND S.PackTypeId = @PackTypeId
                ORDER BY S.DocDate, S.PackNo {direction}";

            var rows = (await _dbConnection.QueryAsync<dynamic>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId, PackTypeId = packTypeId })).ToList();

            // Resolve LotName and PackTypeName via lookups
            var lotLookupList = await _lotMasterLookup.GetByIdsAsync(new[] { lotId });
            var packTypeLookupList = await _packTypeLookup.GetByIdsAsync(new[] { packTypeId });

            if (rows.Count == 0)
                return new List<DispatchAdvicePackRangeDto>();

            // Populate cross-module: ItemName
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            var itemName = items.FirstOrDefault()?.ItemName;

            string? lotName = lotLookupList.FirstOrDefault()?.LotCode;
            string? packTypeName = packTypeLookupList.FirstOrDefault()?.PackTypeName;

            // Preserve SQL order — FIFO ascends, LIFO descends
            var packNos = rows.Select(r => (int)r.PackNo).ToList();

            // Step 1: Group consecutive PackNos (break on gaps).
            // Step direction matches the sort order: +1 for FIFO, -1 for LIFO.
            var step = isLifo ? -1 : 1;
            var consecutiveGroups = new List<List<int>>();
            var currentGroup = new List<int> { packNos[0] };
            for (int i = 1; i < packNos.Count; i++)
            {
                if (packNos[i] == packNos[i - 1] + step)
                {
                    currentGroup.Add(packNos[i]);
                }
                else
                {
                    consecutiveGroups.Add(currentGroup);
                    currentGroup = new List<int> { packNos[i] };
                }
            }
            consecutiveGroups.Add(currentGroup);

            // Step 2: Split each consecutive group into chunks of 'range' size
            var result = new List<DispatchAdvicePackRangeDto>();
            int sNo = 1;
            foreach (var group in consecutiveGroups)
            {
                for (int i = 0; i < group.Count; i += range)
                {
                    var chunk = group.Skip(i).Take(range).ToList();
                    result.Add(new DispatchAdvicePackRangeDto
                    {
                        SNo = sNo++,
                        ItemId = itemId,
                        ItemName = itemName,
                        LotId = lotId,
                        LotName = lotName,
                        PackTypeId = packTypeId,
                        PackTypeName = packTypeName,
                        FromPackNo = chunk.First(),
                        ToPackNo = chunk.Last(),
                        TotalPacks = chunk.Count
                    });
                }
            }

            return result;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAdviceHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> HasInvoiceAsync(int dispatchAdviceId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.InvoiceHeader
                WHERE DispatchAdviceId = @DispatchAdviceId AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { DispatchAdviceId = dispatchAdviceId });
            return count > 0;
        }

        public async Task<IReadOnlyList<DispatchAdviceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, DispatchNo, DispatchDate, InvFlg
                FROM Sales.DispatchAdviceHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                AND DispatchNo LIKE @Term
                ORDER BY DispatchNo ASC";

            var result = await _dbConnection.QueryAsync<DispatchAdviceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<DispatchAdvicePackingListDto?> GetPackingListAsync(int dispatchAdviceId, CancellationToken ct)
        {
            // Header — fetched once (may have zero detail rows if no stock found)
            const string headerSql = @"
                SELECT h.Id AS DispatchAdviceId, h.DispatchNo, h.DispatchDate, h.PartyId
                FROM Sales.DispatchAdviceHeader h
                WHERE h.Id = @DispatchAdviceId AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAdvicePackingListDto>(
                new CommandDefinition(headerSql, new { DispatchAdviceId = dispatchAdviceId }, cancellationToken: ct));

            if (header == null)
                return null;

            // Detail rows — one per pack, join header + detail + stock ledger
            const string detailSql = @"
                SELECT d.ItemId, d.LotId, d.PackTypeId,
                       sl.PackNo, sl.WarehouseId, sl.BinId,
                       sl.TotalQty, sl.TotalValue
                FROM Sales.DispatchAdviceHeader h
                INNER JOIN Sales.DispatchAdviceDetail d ON d.DispatchAdviceHeaderId = h.Id
                INNER JOIN Sales.StockLedger sl
                    ON sl.UnitId = h.UnitId
                    AND sl.ItemId = d.ItemId
                    AND sl.LotId = d.LotId
                    AND sl.PackTypeId = d.PackTypeId
                    AND sl.PackNo BETWEEN d.StartPackNo AND d.EndPackNo
                WHERE h.Id = @DispatchAdviceId
                  AND h.IsDeleted = 0
                ORDER BY d.ItemId, d.LotId, sl.PackNo";

            var details = (await _dbConnection.QueryAsync<DispatchAdvicePackingListDetailDto>(
                new CommandDefinition(detailSql, new { DispatchAdviceId = dispatchAdviceId }, cancellationToken: ct))).ToList();

            // Party lookup (header-level)
            var party = await _partyLookup.GetByIdAsync(header.PartyId, ct);
            header.PartyName = party?.PartyName;
            header.Details = details;

            if (details.Count == 0)
                return header;

            // Cross-module lookups (detail-level) — resolve names once per distinct id set
            var itemIds = details.Select(r => r.ItemId).Distinct().ToList();
            var lotIds = details.Select(r => r.LotId).Distinct().ToList();
            var packTypeIds = details.Select(r => r.PackTypeId).Distinct().ToList();
            var warehouseIds = details.Where(r => r.WarehouseId.HasValue).Select(r => r.WarehouseId!.Value).Distinct().ToList();
            var binIds = details.Where(r => r.BinId.HasValue).Select(r => r.BinId!.Value).Distinct().ToList();

            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var lots = await _lotMasterLookup.GetByIdsAsync(lotIds);
            var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
            var warehouses = warehouseIds.Count > 0 ? await _warehouseLookup.GetByIdsAsync(warehouseIds, ct) : [];
            var bins = binIds.Count > 0 ? await _binLookup.GetByIdsAsync(binIds, ct) : [];

            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);
            var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            foreach (var row in details)
            {
                row.ItemName = itemDict.TryGetValue(row.ItemId, out var iN) ? iN : null;
                row.LotCode = lotDict.TryGetValue(row.LotId, out var lc) ? lc : null;
                row.PackTypeName = packTypeDict.TryGetValue(row.PackTypeId, out var pt) ? pt : null;

                if (row.WarehouseId.HasValue)
                    row.WarehouseName = warehouseDict.TryGetValue(row.WarehouseId.Value, out var wn) ? wn : null;

                if (row.BinId.HasValue)
                    row.BinName = binDict.TryGetValue(row.BinId.Value, out var bn) ? bn : null;
            }

            return header;
        }
    }
}
