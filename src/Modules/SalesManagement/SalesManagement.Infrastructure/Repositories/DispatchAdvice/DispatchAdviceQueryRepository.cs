using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
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
        private readonly IIPAddressService _ipAddressService;

        public DispatchAdviceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _ipAddressService = ipAddressService;
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
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
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

                foreach (var item in list)
                {
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;

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
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
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
                    lm.LotCode,
                    d.StartPackNo, d.EndPackNo, d.DispatchQty
                FROM Sales.DispatchAdviceDetail d
                LEFT JOIN Sales.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                WHERE d.DispatchAdviceHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<DispatchAdviceDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module: PartyName
            var party = await _partyLookup.GetByIdAsync(header.PartyId);
            header.PartyName = party?.PartyName;

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

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
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
                SELECT UnitId
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrderId });
        }

        public async Task<List<DispatchAdviceStockDto>> GetStockAsync(int itemId, int lotId, int statusId)
        {
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
                SELECT Sum(S.TotalQty) AS Qty, Sum(S.TotalValue) AS Value,
                    S.PackTypeId, P.PackTypeCode, P.PackTypeName,
                    P.NetWeight, P.TareWeight, P.GrossWeight, P.ConesPerBag
                FROM Sales.StockLedger S
                INNER JOIN Sales.PackType P ON S.PackTypeId = P.Id
                WHERE S.UnitId = @UnitId AND S.ItemId = @ItemId AND S.StatusId = @StatusId AND S.LotId = @LotId
                GROUP BY S.PackTypeId, P.PackTypeCode, P.PackTypeName,
                    P.NetWeight, P.TareWeight, P.GrossWeight, P.ConesPerBag";

            var result = await _dbConnection.QueryAsync<DispatchAdviceStockDto>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId });
            return result.ToList();
        }


        public async Task<List<int>> GetAvailablePackNosAsync(int itemId, int lotId, int statusId, int startPackNo, int endPackNo, int packTypeId)
        {
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
                SELECT PackNo
                FROM Sales.StockLedger
                WHERE UnitId = @UnitId AND ItemId = @ItemId AND StatusId = @StatusId AND LotId = @LotId
                AND PackNo BETWEEN @StartPackNo AND @EndPackNo AND PackTypeId = @PackTypeId";

            var result = await _dbConnection.QueryAsync<int>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId, StartPackNo = startPackNo, EndPackNo = endPackNo, PackTypeId = packTypeId });
            return result.ToList();
        }

        public async Task<IReadOnlyList<DispatchAdviceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, DispatchNo, DispatchDate
                FROM Sales.DispatchAdviceHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                AND DispatchNo LIKE @Term
                ORDER BY DispatchNo ASC";

            var result = await _dbConnection.QueryAsync<DispatchAdviceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }
    }
}
