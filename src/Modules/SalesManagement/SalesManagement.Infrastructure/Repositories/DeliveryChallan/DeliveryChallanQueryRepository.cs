using System.Data;
using Contracts.Interfaces;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Infrastructure.Repositories.DeliveryChallan
{
    public class DeliveryChallanQueryRepository : IDeliveryChallanQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IIPAddressService _ipAddressService;

        public DeliveryChallanQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<DeliveryChallanHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.DeliveryNumber LIKE @SearchTerm
                       OR sh.StoNumber LIKE @SearchTerm
                       OR ms.Description LIKE @SearchTerm
                       OR h.VehicleNumber LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.DeliveryNumber,
                    h.DeliveryDate,
                    h.StoHeaderId,
                    sh.StoNumber,
                    h.FromPlantId,
                    h.FromStorageLocationId,
                    h.ToPlantId,
                    h.ToStorageLocationId,
                    h.TransporterId,
                    h.VehicleNumber,
                    h.TransportDistance,
                    h.DeliveryValue,
                    h.ConsignmentValue,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(countSql + dataSql, parameters);
            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<DeliveryChallanHeaderDto>()).ToList();

            // Populate cross-module FK names via lookups
            if (data.Count > 0)
            {
                var allPlantIds = data.Select(d => d.FromPlantId)
                    .Union(data.Select(d => d.ToPlantId))
                    .Distinct();
                var plants = await _unitLookup.GetByIdsAsync(allPlantIds);
                var plantDict = plants.ToDictionary(p => p.UnitId, p => p.UnitName);

                var warehouseIds = data.Select(d => d.FromStorageLocationId)
                    .Union(data.Select(d => d.ToStorageLocationId))
                    .Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                var transporterIds = data.Select(d => d.TransporterId).Distinct();
                var transporters = await _partyLookup.GetByIdsAsync(transporterIds);
                var transporterDict = transporters.ToDictionary(t => t.Id, t => t.PartyName);

                foreach (var item in data)
                {
                    item.FromPlantName = plantDict.TryGetValue(item.FromPlantId, out var fpName) ? fpName : null;
                    item.ToPlantName = plantDict.TryGetValue(item.ToPlantId, out var tpName) ? tpName : null;
                    item.FromStorageLocationName = warehouseDict.TryGetValue(item.FromStorageLocationId, out var fsName) ? fsName : null;
                    item.ToStorageLocationName = warehouseDict.TryGetValue(item.ToStorageLocationId, out var tsName) ? tsName : null;
                    item.TransporterName = transporterDict.TryGetValue(item.TransporterId, out var trName) ? trName : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<DeliveryChallanHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id,
                    h.DeliveryNumber,
                    h.DeliveryDate,
                    h.StoHeaderId,
                    sh.StoNumber,
                    h.FromPlantId,
                    h.FromStorageLocationId,
                    h.ToPlantId,
                    h.ToStorageLocationId,
                    h.TransporterId,
                    h.VehicleNumber,
                    h.TransportDistance,
                    h.DeliveryValue,
                    h.ConsignmentValue,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DeliveryChallanHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Populate cross-module header lookups
            var fromPlant = await _unitLookup.GetByIdAsync(header.FromPlantId);
            header.FromPlantName = fromPlant?.UnitName;

            var toPlant = await _unitLookup.GetByIdAsync(header.ToPlantId);
            header.ToPlantName = toPlant?.UnitName;

            var warehouses = await _warehouseLookup.GetByIdsAsync(
                new[] { header.FromStorageLocationId, header.ToStorageLocationId });
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);
            header.FromStorageLocationName = warehouseDict.TryGetValue(header.FromStorageLocationId, out var fsName) ? fsName : null;
            header.ToStorageLocationName = warehouseDict.TryGetValue(header.ToStorageLocationId, out var tsName) ? tsName : null;

            var transporter = await _partyLookup.GetByIdAsync(header.TransporterId);
            header.TransporterName = transporter?.PartyName;

            // Fetch details with Lot JOIN
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.DeliveryChallanHeaderId,
                    d.StoDetailId,
                    d.ItemId,
                    d.LotId,
                    lm.LotCode,
                    d.StartPackNo,
                    d.EndPackNo,
                    d.DispatchQuantity,
                    d.UOMId,
                    d.BagCount,
                    d.BaleCount,
                    d.NetWeight,
                    d.GrossWeight,
                    d.ExMillRate,
                    d.LineMovementValue
                FROM Sales.DeliveryChallanDetail d
                LEFT JOIN Production.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                WHERE d.DeliveryChallanHeaderId = @HeaderId;";

            var details = (await _dbConnection.QueryAsync<DeliveryChallanDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module detail lookups (Item, UOM)
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var uomIds = details.Select(d => d.UOMId).Distinct();
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
                    detail.UOMName = uomDict.TryGetValue(detail.UOMId, out var uomName) ? uomName : null;
                }
            }

            header.DeliveryChallanDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<DeliveryChallanLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT TOP 20
                    h.Id,
                    h.DeliveryNumber,
                    h.DeliveryDate,
                    sh.StoNumber
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                  AND h.FromPlantId = @UnitId
                  AND (h.DeliveryNumber LIKE @Term OR sh.StoNumber LIKE @Term)
                ORDER BY h.DeliveryNumber ASC;";

            var result = await _dbConnection.QueryAsync<DeliveryChallanLookupDto>(sql, new { UnitId = unitId, Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DeliveryChallanHeader
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> StoHeaderExistsAsync(int stoHeaderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoHeader
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = stoHeaderId });
            return count > 0;
        }

        public async Task<bool> LotExistsAsync(int lotId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.LotMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = lotId });
            return count > 0;
        }

        public async Task<StoOpenQtyDto?> GetStoOpenQtyAsync(int stoDetailId)
        {
            const string sql = @"
                SELECT
                    sd.Id AS StoDetailId,
                    sd.ItemId,
                    sd.Quantity AS OrderedQty,
                    ISNULL(SUM(dcd.DispatchQuantity), 0) AS DispatchedQty,
                    sd.Quantity - ISNULL(SUM(dcd.DispatchQuantity), 0) AS OpenQty,
                    sd.UOMId,
                    sd.TransferPrice
                FROM Sales.StoDetail sd
                LEFT JOIN Sales.DeliveryChallanDetail dcd ON sd.Id = dcd.StoDetailId
                    AND dcd.DeliveryChallanHeaderId IN (
                        SELECT Id FROM Sales.DeliveryChallanHeader WHERE IsDeleted = 0
                    )
                WHERE sd.Id = @StoDetailId
                GROUP BY sd.Id, sd.ItemId, sd.Quantity, sd.UOMId, sd.TransferPrice;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<StoOpenQtyDto>(sql, new { StoDetailId = stoDetailId });

            if (result != null)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { result.ItemId });
                var item = items.FirstOrDefault();
                result.ItemName = item?.ItemName;

                var uoms = await _uomLookup.GetByIdsAsync(new[] { result.UOMId });
                var uom = uoms.FirstOrDefault();
                result.UOMName = uom?.UOMName;
            }

            return result;
        }

        public async Task<(List<DeliveryChallanHeaderDto>, int)> GetPendingAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var currentUserId = _ipAddressService.GetUserId();

            // Get Pending status Id from ApprovalStatus MiscType (Sales module)
            const string pendingIdSql = @"
                SELECT m.Id FROM Sales.MiscMaster m
                INNER JOIN Sales.MiscTypeMaster mt ON mt.Id = m.MiscTypeId AND mt.IsDeleted = 0
                WHERE m.Code = 'Pending' AND mt.MiscTypeCode = 'ApprovalStatus' AND m.IsDeleted = 0;";

            var pendingStatusId = await _dbConnection.ExecuteScalarAsync<int?>(pendingIdSql);
            if (!pendingStatusId.HasValue)
                return (new List<DeliveryChallanHeaderDto>(), 0);

            // Get Pending status Id from AppData MiscMaster (Workflow module)
            const string appPendingIdSql = @"
                SELECT m.Id FROM AppData.MiscMaster m
                INNER JOIN AppData.MiscTypeMaster mt ON mt.Id = m.MiscTypeId AND mt.IsDeleted = 0
                WHERE m.Code = 'Pending' AND mt.MiscTypeCode = 'ApprovalStatus' AND m.IsDeleted = 0;";

            var appPendingStatusId = await _dbConnection.ExecuteScalarAsync<int?>(appPendingIdSql);
            if (!appPendingStatusId.HasValue)
                return (new List<DeliveryChallanHeaderDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.DeliveryNumber LIKE @SearchTerm
                       OR sh.StoNumber LIKE @SearchTerm
                       OR ms.Description LIKE @SearchTerm
                       OR h.VehicleNumber LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            // Filter by logged-in approver via AppData.ApprovalRequest
            const string approverFilter = @"
                AND h.Id IN (
                    SELECT ar.ModuleTransactionId
                    FROM AppData.ApprovalRequest ar
                    WHERE ar.WorkflowType = 'Delivery Challan'
                      AND ar.ApproverValue = @CurrentUserId
                      AND ar.StatusId = @AppPendingStatusId
                )";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.StatusId = @PendingStatusId {approverFilter} {searchFilter};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.DeliveryNumber,
                    h.DeliveryDate,
                    h.StoHeaderId,
                    sh.StoNumber,
                    h.FromPlantId,
                    h.FromStorageLocationId,
                    h.ToPlantId,
                    h.ToStorageLocationId,
                    h.TransporterId,
                    h.VehicleNumber,
                    h.TransportDistance,
                    h.DeliveryValue,
                    h.ConsignmentValue,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.StatusId = @PendingStatusId {approverFilter} {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                PendingStatusId = pendingStatusId.Value,
                AppPendingStatusId = appPendingStatusId.Value,
                CurrentUserId = currentUserId,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(countSql + dataSql, parameters);
            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<DeliveryChallanHeaderDto>()).ToList();

            // Populate cross-module FK names via lookups
            if (data.Count > 0)
            {
                var allPlantIds = data.Select(d => d.FromPlantId)
                    .Union(data.Select(d => d.ToPlantId))
                    .Distinct();
                var plants = await _unitLookup.GetByIdsAsync(allPlantIds);
                var plantDict = plants.ToDictionary(p => p.UnitId, p => p.UnitName);

                var warehouseIds = data.Select(d => d.FromStorageLocationId)
                    .Union(data.Select(d => d.ToStorageLocationId))
                    .Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                var transporterIds = data.Select(d => d.TransporterId).Distinct();
                var transporters = await _partyLookup.GetByIdsAsync(transporterIds);
                var transporterDict = transporters.ToDictionary(t => t.Id, t => t.PartyName);

                foreach (var item in data)
                {
                    item.FromPlantName = plantDict.TryGetValue(item.FromPlantId, out var fpName) ? fpName : null;
                    item.ToPlantName = plantDict.TryGetValue(item.ToPlantId, out var tpName) ? tpName : null;
                    item.FromStorageLocationName = warehouseDict.TryGetValue(item.FromStorageLocationId, out var fsName) ? fsName : null;
                    item.ToStorageLocationName = warehouseDict.TryGetValue(item.ToStorageLocationId, out var tsName) ? tsName : null;
                    item.TransporterName = transporterDict.TryGetValue(item.TransporterId, out var trName) ? trName : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<bool> HasStoReceiptAsync(int dcHeaderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoReceiptHeader
                WHERE DeliveryChallanHeaderId = @DcHeaderId AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { DcHeaderId = dcHeaderId });
            return count > 0;
        }

        public async Task<bool> IsStoApprovedAsync(int stoHeaderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoHeader sh
                INNER JOIN Sales.MiscMaster mm ON sh.HeaderStatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE sh.Id = @Id AND sh.IsDeleted = 0
                  AND mt.MiscTypeCode = 'ApprovalStatus'
                  AND mm.Code = 'Approved';";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = stoHeaderId });
            return count > 0;
        }

        public async Task<bool> IsStoFullyDispatchedAsync(int stoHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(*) = 0 THEN 1 ELSE 0 END
                FROM Sales.StoDetail sd
                LEFT JOIN (
                    SELECT dcd.StoDetailId, SUM(dcd.DispatchQuantity) AS TotalDispatched
                    FROM Sales.DeliveryChallanDetail dcd
                    INNER JOIN Sales.DeliveryChallanHeader dch ON dcd.DeliveryChallanHeaderId = dch.Id AND dch.IsDeleted = 0
                    GROUP BY dcd.StoDetailId
                ) dispatched ON sd.Id = dispatched.StoDetailId
                WHERE sd.StoHeaderId = @StoHeaderId AND sd.IsDeleted = 0
                  AND (sd.Quantity - ISNULL(dispatched.TotalDispatched, 0)) > 0;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(sql, new { StoHeaderId = stoHeaderId });
            return result == 1;
        }
    }
}
