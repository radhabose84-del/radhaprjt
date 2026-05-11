using System.Data;
using Contracts.Interfaces;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending;
using SalesManagement.Domain.Common;
using Contracts.Dtos.Lookups.Party;

namespace SalesManagement.Infrastructure.Repositories.DeliveryChallan
{
    public class DeliveryChallanQueryRepository : IDeliveryChallanQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ILotMasterLookup _lotLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IUserLookup _userLookup;
        private readonly IEWaybillLookup _eWaybillLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyDetailLookup _companyDetailLookup;
        private readonly IUnitDetailLookup _unitDetailLookup;
        private readonly IPartyDetailLookup _partyDetailLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;

        public DeliveryChallanQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            ILotMasterLookup lotLookup,
            IUOMLookup uomLookup,
            IUserLookup userLookup,
            IEWaybillLookup eWaybillLookup,
            IIPAddressService ipAddressService,
            ICompanyDetailLookup companyDetailLookup,
            IUnitDetailLookup unitDetailLookup,
            IPartyDetailLookup partyDetailLookup,
            IStateLookup stateLookup,
            ICityLookup cityLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _lotLookup = lotLookup;
            _uomLookup = uomLookup;
            _userLookup = userLookup;
            _eWaybillLookup = eWaybillLookup;
            _ipAddressService = ipAddressService;
            _companyDetailLookup = companyDetailLookup;
            _unitDetailLookup = unitDetailLookup;
            _partyDetailLookup = partyDetailLookup;
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
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
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
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
                    h.DcTypeId,
                    dt.Description AS DcTypeName,
                    h.MovementTypeId,
                    mt.MovementCode,
                    mt.MovementDescription,
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
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
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

                    if (!string.IsNullOrWhiteSpace(item.DeliveryNumber))
                    {
                        var ewaybill = await _eWaybillLookup.GetByDCAsync(item.DeliveryNumber, item.FromPlantId);
                        item.EWaybillExists = ewaybill != null;
                    }
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
                    h.DcTypeId,
                    dt.Description AS DcTypeName,
                    h.MovementTypeId,
                    mt.MovementCode,
                    mt.MovementDescription,
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
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
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

            // Fetch details (same-module only)
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.DeliveryChallanHeaderId,
                    d.StoDetailId,
                    d.ItemId,
                    d.LotId,
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
                WHERE d.DeliveryChallanHeaderId = @HeaderId;";

            var details = (await _dbConnection.QueryAsync<DeliveryChallanDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module detail lookups (Item, Lot, UOM)
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var lotIds = details.Where(d => d.LotId > 0).Select(d => d.LotId).Distinct();
                var lots = await _lotLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

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
                    detail.LotCode = lotDict.TryGetValue(detail.LotId, out var lotCode) ? lotCode : null;
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

        public async Task<IReadOnlyList<DeliveryChallanLookupDto>> GetForReceiptAsync(string term, CancellationToken ct)
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
                  AND h.ToPlantId = @UnitId
                  AND NOT EXISTS (
                      SELECT 1 FROM Sales.StoReceiptHeader sr
                      WHERE sr.DeliveryChallanHeaderId = h.Id AND sr.IsDeleted = 0
                  )
                  AND (h.DeliveryNumber LIKE @Term OR sh.StoNumber LIKE @Term)
                ORDER BY h.DeliveryNumber ASC;";

            var result = await _dbConnection.QueryAsync<DeliveryChallanLookupDto>(
                new CommandDefinition(sql, new { UnitId = unitId, Term = $"%{term}%" }, cancellationToken: ct));
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
            var lots = await _lotLookup.GetByIdsAsync(new[] { lotId });
            return lots.Count > 0;
        }

        public async Task<bool> DcTypeExistsAsync(int dcTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mm.Id = @Id
                  AND mt.MiscTypeCode = 'DCType'
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mt.IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dcTypeId });
            return count > 0;
        }

        public async Task<bool> MovementTypeConfigExistsAsync(int movementTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MovementTypeConfig
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = movementTypeId });
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
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
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
                    h.DcTypeId,
                    dt.Description AS DcTypeName,
                    h.MovementTypeId,
                    mt.MovementCode,
                    mt.MovementDescription,
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
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
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

                // Workflow enrichment: fetch ApprovalRequestId and ApproverValue for each record
                var moduleIds = data.Select(d => d.Id).ToList();
                const string approvalSql = @"
                    SELECT ar.ModuleTransactionId, ar.Id AS ApprovalRequestId, ar.ApproverValue
                    FROM AppData.ApprovalRequest ar
                    WHERE ar.WorkflowType = 'Delivery Challan'
                      AND ar.ApproverValue = @CurrentUserId
                      AND ar.StatusId = @AppPendingStatusId
                      AND ar.ModuleTransactionId IN @ModuleIds;";

                var approvals = (await _dbConnection.QueryAsync<dynamic>(approvalSql, new
                {
                    CurrentUserId = currentUserId,
                    AppPendingStatusId = appPendingStatusId.Value,
                    ModuleIds = moduleIds
                })).ToList();

                var approvalDict = approvals.ToDictionary(
                    a => (int)a.ModuleTransactionId,
                    a => (ApprovalRequestId: (int)a.ApprovalRequestId, ApproverValue: Convert.ToInt32(a.ApproverValue)));

                // Resolve approver names
                var users = await _userLookup.GetAllUserAsync();
                var userNameMap = users.ToDictionary(u => u.UserId, u => u.UserName ?? string.Empty);

                foreach (var item in data)
                {
                    item.FromPlantName = plantDict.TryGetValue(item.FromPlantId, out var fpName) ? fpName : null;
                    item.ToPlantName = plantDict.TryGetValue(item.ToPlantId, out var tpName) ? tpName : null;
                    item.FromStorageLocationName = warehouseDict.TryGetValue(item.FromStorageLocationId, out var fsName) ? fsName : null;
                    item.ToStorageLocationName = warehouseDict.TryGetValue(item.ToStorageLocationId, out var tsName) ? tsName : null;
                    item.TransporterName = transporterDict.TryGetValue(item.TransporterId, out var trName) ? trName : null;

                    if (approvalDict.TryGetValue(item.Id, out var approval))
                    {
                        item.ApprovalRequestHeaderId = approval.ApprovalRequestId;
                        item.ApproverId = approval.ApproverValue;
                        if (userNameMap.TryGetValue(item.ApproverId, out var name))
                            item.ApproverName = name;
                    }
                }
            }

            return (data, totalCount);
        }

        public async Task<DeliveryChallanHeaderDto?> GetPendingByIdAsync(int id)
        {
            var currentUserId = _ipAddressService.GetUserId();

            // Get Pending status Id from ApprovalStatus MiscType (Sales module)
            const string pendingIdSql = @"
                SELECT m.Id FROM Sales.MiscMaster m
                INNER JOIN Sales.MiscTypeMaster mt ON mt.Id = m.MiscTypeId AND mt.IsDeleted = 0
                WHERE m.Code = 'Pending' AND mt.MiscTypeCode = 'ApprovalStatus' AND m.IsDeleted = 0;";

            var pendingStatusId = await _dbConnection.ExecuteScalarAsync<int?>(pendingIdSql);
            if (!pendingStatusId.HasValue)
                return null;

            // Get Pending status Id from AppData MiscMaster (Workflow module)
            const string appPendingIdSql = @"
                SELECT m.Id FROM AppData.MiscMaster m
                INNER JOIN AppData.MiscTypeMaster mt ON mt.Id = m.MiscTypeId AND mt.IsDeleted = 0
                WHERE m.Code = 'Pending' AND mt.MiscTypeCode = 'ApprovalStatus' AND m.IsDeleted = 0;";

            var appPendingStatusId = await _dbConnection.ExecuteScalarAsync<int?>(appPendingIdSql);
            if (!appPendingStatusId.HasValue)
                return null;

            const string headerSql = @"
                SELECT
                    h.Id, h.DeliveryNumber, h.DeliveryDate,
                    h.StoHeaderId, sh.StoNumber,
                    h.FromPlantId, h.FromStorageLocationId,
                    h.ToPlantId, h.ToStorageLocationId,
                    h.TransporterId, h.VehicleNumber, h.TransportDistance,
                    h.DeliveryValue, h.ConsignmentValue,
                    h.StatusId, ms.Description AS StatusName,
                    h.DcTypeId, dt.Description AS DcTypeName,
                    h.MovementTypeId, mt.MovementCode, mt.MovementDescription,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0 AND h.StatusId = @PendingStatusId
                  AND h.Id IN (
                      SELECT ar.ModuleTransactionId
                      FROM AppData.ApprovalRequest ar
                      WHERE ar.WorkflowType = 'Delivery Challan'
                        AND ar.ApproverValue = @CurrentUserId
                        AND ar.StatusId = @AppPendingStatusId
                  );";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DeliveryChallanHeaderDto>(
                headerSql, new { Id = id, PendingStatusId = pendingStatusId.Value, AppPendingStatusId = appPendingStatusId.Value, CurrentUserId = currentUserId });

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

            // Fetch details
            const string detailSql = @"
                SELECT
                    d.Id, d.DeliveryChallanHeaderId, d.StoDetailId,
                    d.ItemId, d.LotId, d.StartPackNo, d.EndPackNo,
                    d.DispatchQuantity, d.UOMId,
                    d.BagCount, d.BaleCount,
                    d.NetWeight, d.GrossWeight,
                    d.ExMillRate, d.LineMovementValue
                FROM Sales.DeliveryChallanDetail d
                WHERE d.DeliveryChallanHeaderId = @HeaderId;";

            var details = (await _dbConnection.QueryAsync<DeliveryChallanDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module detail lookups (Item, Lot, UOM)
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var lotIds = details.Where(d => d.LotId > 0).Select(d => d.LotId).Distinct();
                var lots = await _lotLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

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
                    detail.LotCode = lotDict.TryGetValue(detail.LotId, out var lotCode) ? lotCode : null;
                    detail.UOMName = uomDict.TryGetValue(detail.UOMId, out var uomName) ? uomName : null;
                }
            }

            header.DeliveryChallanDetails = details;
            return header;
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
                WHERE sd.StoHeaderId = @StoHeaderId
                  AND (sd.Quantity - ISNULL(dispatched.TotalDispatched, 0)) > 0;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(sql, new { StoHeaderId = stoHeaderId });
            return result == 1;
        }

        public async Task<List<GetDCGatePassPendingDto>> GetDCGatePassPendingAsync(string? vehicleNo)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                -- Resolve Approved StatusId from ApprovalStatus
                DECLARE @ApprovedStatusId INT;
                SELECT @ApprovedStatusId = mm.Id
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mt.Description = @MiscType
                  AND mm.Code = @StatusCode
                  AND mm.IsDeleted = 0;

                -- Result set 1: Headers
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
                    h.GEFlag,
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
                WHERE h.IsDeleted = 0 AND h.GEFlag = 0
                  AND h.FromPlantId = @UnitId
                  AND h.StatusId = @ApprovedStatusId
                  AND (@VehicleNo IS NULL OR h.VehicleNumber LIKE '%' + @VehicleNo + '%')
                ORDER BY h.Id DESC;

                -- Result set 2: Detail rows for all matching headers
                SELECT
                    d.DeliveryChallanHeaderId AS DCHeaderId,
                    d.StoDetailId,
                    d.ItemId,
                    d.LotId,
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
                INNER JOIN Sales.DeliveryChallanHeader h ON d.DeliveryChallanHeaderId = h.Id
                WHERE h.IsDeleted = 0 AND h.GEFlag = 0
                  AND h.FromPlantId = @UnitId
                  AND h.StatusId = @ApprovedStatusId
                  AND (@VehicleNo IS NULL OR h.VehicleNumber LIKE '%' + @VehicleNo + '%')
                ORDER BY d.DeliveryChallanHeaderId DESC;
            ";

            var parameters = new
            {
                UnitId = unitId,
                VehicleNo = string.IsNullOrWhiteSpace(vehicleNo) ? null : vehicleNo,
                MiscType = MiscEnumEntity.StoApprovalStatus,
                StatusCode = MiscEnumEntity.StoApprovalApproved
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);

            var headers = (await multi.ReadAsync<GetDCGatePassPendingDto>()).ToList();
            var details = (await multi.ReadAsync<GetDCGatePassPendingDto.GetDCGatePassPendingDetailDto>()).ToList();

            // Group details into their parent headers
            var detailLookup = details.ToLookup(d => d.DCHeaderId);
            foreach (var h in headers)
            {
                h.DeliveryChallanDetails = detailLookup[h.Id].ToList();
            }

            return headers;
        }

        public async Task<DeliveryChallanPrintDto?> GetPrintDetailsAsync(int id)
        {
            // 1. Fetch DC header with same-module JOINs
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
                    ms.Description AS StatusName,
                    dt.Description AS DcTypeName,
                    mt.MovementCode,
                    mt.MovementDescription,
                    h.Remarks,
                    h.CreatedDate
                FROM Sales.DeliveryChallanHeader h
                LEFT JOIN Sales.StoHeader sh ON h.StoHeaderId = sh.Id AND sh.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DcTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DCPrintHeaderRawDto>(headerSql, new { Id = id });
            if (header == null)
                return null;

            // 2. Fetch DC line items
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.ItemId,
                    d.LotId,
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
                WHERE d.DeliveryChallanHeaderId = @HeaderId
                ORDER BY d.Id";

            var details = (await _dbConnection.QueryAsync<DCPrintDetailRawDto>(detailSql, new { HeaderId = id })).ToList();

            // 3. Cross-module lookups — company and plants
            var company = await _companyDetailLookup.GetByUnitIdAsync(header.FromPlantId);
            var fromUnit = await _unitDetailLookup.GetByIdAsync(header.FromPlantId);
            var toUnit = await _unitDetailLookup.GetByIdAsync(header.ToPlantId);

            // 4. Transporter details via party detail lookup
            Contracts.Dtos.Lookups.Party.PartyDetailLookupDto? transporter = null;
            if (header.TransporterId > 0)
                transporter = await _partyDetailLookup.GetByIdAsync(header.TransporterId);

            // 5. E-Waybill via lookup
            var eway = !string.IsNullOrWhiteSpace(header.DeliveryNumber)
                ? await _eWaybillLookup.GetByDCAsync(header.DeliveryNumber, header.FromPlantId)
                : null;

            // 6. Collect all city/state IDs for batch resolution
            var stateIds = new HashSet<int>();
            var cityIds = new HashSet<int>();

            if (company != null) { if (company.StateId > 0) stateIds.Add(company.StateId); if (company.CityId > 0) cityIds.Add(company.CityId); }
            if (fromUnit != null) { if (fromUnit.StateId > 0) stateIds.Add(fromUnit.StateId); if (fromUnit.CityId > 0) cityIds.Add(fromUnit.CityId); }
            if (toUnit != null) { if (toUnit.StateId > 0) stateIds.Add(toUnit.StateId); if (toUnit.CityId > 0) cityIds.Add(toUnit.CityId); }

            var states = await _stateLookup.GetByIdsAsync(stateIds);
            var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);

            var cities = await _cityLookup.GetByIdsAsync(cityIds);
            var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);

            // 7. Item, Lot, UOM lookups for line items
            var itemIds = details.Select(d => d.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

            var lotIds = details.Where(d => d.LotId > 0).Select(d => d.LotId).Distinct();
            var lots = await _lotLookup.GetByIdsAsync(lotIds);
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

            var uomIds = details.Select(d => d.UOMId).Distinct();
            var uoms = await _uomLookup.GetByIdsAsync(uomIds);
            var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

            // --- Assemble DTO ---

            // Company section (sending unit's company)
            var companyAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(company?.AddressLine1)) companyAddrParts.Add(company.AddressLine1);
            if (!string.IsNullOrWhiteSpace(company?.AddressLine2)) companyAddrParts.Add(company.AddressLine2);

            var companyCity = company?.CityId > 0 && cityDict.TryGetValue(company.CityId, out var cc) ? cc : null;
            var companyState = company?.StateId > 0 && stateDict.TryGetValue(company.StateId, out var cs) ? cs : null;

            var companyDto = new DCPrintCompanyDto
            {
                Name = company != null
                    ? $"{company.LegalName ?? company.CompanyName} {fromUnit?.UnitName}".Trim()
                    : fromUnit?.UnitName,
                Address = string.Join(", ", companyAddrParts),
                City = companyCity != null
                    ? $"{companyCity}{(!string.IsNullOrWhiteSpace(company?.PinCode) ? " - " + company.PinCode : "")}"
                    : null,
                State = companyState,
                Gstin = company?.GstNumber,
                Pan = company?.PanNumber,
                Email = company?.Email,
                Web = company?.Website,
                Phone = company?.Phone
            };

            // Registered office (company HQ)
            var regAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(company?.AddressLine1)) regAddrParts.Add(company.AddressLine1);
            if (!string.IsNullOrWhiteSpace(company?.AddressLine2)) regAddrParts.Add(company.AddressLine2);

            DCPrintRegisteredOfficeDto? registeredOffice = company != null
                ? new DCPrintRegisteredOfficeDto
                {
                    Name = company.LegalName ?? company.CompanyName,
                    Address = string.Join(", ", regAddrParts),
                    City = companyCity != null
                        ? $"{companyCity}{(companyState != null ? " " + companyState : "")}{(!string.IsNullOrWhiteSpace(company.PinCode) ? " - " + company.PinCode : "")}"
                        : null,
                    Phone = company.Phone
                }
                : null;

            // DC header section
            var headerDto = new DCPrintHeaderDto
            {
                DeliveryNumber = header.DeliveryNumber,
                DeliveryDate = header.DeliveryDate.ToString("dd/MM/yyyy"),
                StoNumber = header.StoNumber,
                DcType = header.DcTypeName,
                MovementCode = header.MovementCode,
                MovementDescription = header.MovementDescription,
                Status = header.StatusName,
                DateTimeOfSupply = header.CreatedDate?.ToString("dd/MM/yyyy hh:mm tt")
            };

            // E-Waybill section
            DCPrintEWaybillDto? ewaybillDto = eway != null
                ? new DCPrintEWaybillDto
                {
                    EWBNumber = eway.EWBNumber,
                    EwbStatus = eway.EwbStatus,
                    GeneratedDate = eway.GeneratedDate?.ToString("dd/MM/yyyy HH:mm")
                }
                : null;

            // From plant
            var fromAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(fromUnit?.AddressLine1)) fromAddrParts.Add(fromUnit.AddressLine1);
            if (!string.IsNullOrWhiteSpace(fromUnit?.AddressLine2)) fromAddrParts.Add(fromUnit.AddressLine2);

            var fromCity = fromUnit?.CityId > 0 && cityDict.TryGetValue(fromUnit.CityId, out var fc) ? fc : null;
            var fromState = fromUnit?.StateId > 0 && stateDict.TryGetValue(fromUnit.StateId, out var fs) ? fs : null;

            var fromDto = new DCPrintPlantDto
            {
                UnitName = fromUnit?.UnitName,
                Address = string.Join(", ", fromAddrParts),
                City = fromCity != null
                    ? $"{fromCity}{(fromUnit?.PinCode > 0 ? " - " + fromUnit.PinCode : "")}"
                    : null,
                State = fromState,
                Phone = fromUnit?.Phone
            };

            // To plant
            var toAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(toUnit?.AddressLine1)) toAddrParts.Add(toUnit.AddressLine1);
            if (!string.IsNullOrWhiteSpace(toUnit?.AddressLine2)) toAddrParts.Add(toUnit.AddressLine2);

            var toCity = toUnit?.CityId > 0 && cityDict.TryGetValue(toUnit.CityId, out var tc) ? tc : null;
            var toState = toUnit?.StateId > 0 && stateDict.TryGetValue(toUnit.StateId, out var ts) ? ts : null;

            var toDto = new DCPrintPlantDto
            {
                UnitName = toUnit?.UnitName,
                Address = string.Join(", ", toAddrParts),
                City = toCity != null
                    ? $"{toCity}{(toUnit?.PinCode > 0 ? " - " + toUnit.PinCode : "")}"
                    : null,
                State = toState,
                Phone = toUnit?.Phone
            };

            // Transport
            var transportDto = new DCPrintTransportDto
            {
                TransporterName = transporter?.PartyName,
                TransporterCode = transporter?.PartyCode,
                TransporterGstin = transporter?.GSTNumber,
                VehicleNo = header.VehicleNumber,
                TransportDistance = header.TransportDistance
            };

            // Line items
            var printItems = details.Select((d, index) =>
            {
                var itemInfo = itemDict.TryGetValue(d.ItemId, out var ii) ? ii : default;
                var lotNo = d.LotId > 0 && lotDict.TryGetValue(d.LotId, out var lc) ? lc : null;
                var uomName = uomDict.TryGetValue(d.UOMId, out var un) ? un : null;
                var packSNo = d.StartPackNo == d.EndPackNo
                    ? d.StartPackNo.ToString()
                    : $"{d.StartPackNo}-{d.EndPackNo}";

                return new DCPrintItemDto
                {
                    SNo = index + 1,
                    ItemCode = itemInfo.ItemCode,
                    ItemName = itemInfo.ItemName,
                    LotNo = lotNo,
                    PackSerialNo = packSNo,
                    BagCount = d.BagCount,
                    BaleCount = d.BaleCount,
                    DispatchQuantity = d.DispatchQuantity,
                    UOMName = uomName,
                    NetWeight = d.NetWeight,
                    GrossWeight = d.GrossWeight,
                    ExMillRate = d.ExMillRate,
                    LineMovementValue = d.LineMovementValue
                };
            }).ToList();

            // Totals
            var totalsDto = new DCPrintTotalsDto
            {
                TotalBags = details.Sum(d => d.BagCount ?? 0),
                TotalBales = details.Sum(d => d.BaleCount ?? 0),
                TotalQuantity = details.Sum(d => d.DispatchQuantity),
                TotalNetWeight = details.Sum(d => d.NetWeight),
                TotalGrossWeight = details.Sum(d => d.GrossWeight ?? 0),
                DeliveryValue = header.DeliveryValue,
                ConsignmentValue = header.ConsignmentValue,
                DeliveryValueWords = ConvertAmountToWords(header.DeliveryValue),
                Remarks = header.Remarks
            };

            return new DeliveryChallanPrintDto
            {
                Company = companyDto,
                RegisteredOffice = registeredOffice,
                Header = headerDto,
                EWaybill = ewaybillDto,
                From = fromDto,
                To = toDto,
                Transport = transportDto,
                Items = printItems,
                Totals = totalsDto
            };
        }

        private static string ConvertAmountToWords(decimal amount)
        {
            var wholeAmount = (long)Math.Floor(Math.Abs(amount));
            if (wholeAmount == 0)
                return "Rs. Zero only";

            var result = ConvertNumberToWords(wholeAmount);
            return $"Rs. {result} only";
        }

        private static string ConvertNumberToWords(long number)
        {
            if (number == 0) return "zero";

            var parts = new List<string>();

            if (number >= 10000000)
            {
                parts.Add(ConvertNumberToWords(number / 10000000) + " crore");
                number %= 10000000;
            }
            if (number >= 100000)
            {
                parts.Add(ConvertNumberToWords(number / 100000) + " lakh");
                number %= 100000;
            }
            if (number >= 1000)
            {
                parts.Add(ConvertNumberToWords(number / 1000) + " thousand");
                number %= 1000;
            }
            if (number >= 100)
            {
                parts.Add(ConvertNumberToWords(number / 100) + " hundred");
                number %= 100;
            }
            if (number > 0)
            {
                if (parts.Count > 0) parts.Add("and");

                string[] ones = { "", "one", "two", "three", "four", "five", "six", "seven",
                    "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen",
                    "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                string[] tens = { "", "", "twenty", "thirty", "forty", "fifty",
                    "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                {
                    parts.Add(ones[number]);
                }
                else
                {
                    var tenPart = tens[number / 10];
                    var onePart = ones[number % 10];
                    parts.Add(string.IsNullOrEmpty(onePart) ? tenPart : $"{tenPart}-{onePart}");
                }
            }

            var text = string.Join(" ", parts);
            return char.ToUpper(text[0]) + text[1..];
        }

        // Build a print-ready EWB document view by joining DC + EWB header + EWB lines +
        // unit/company/address master data. Single round-trip via QueryMultiple to keep
        // page load fast for the UI's "Print EWB" button.
        public async Task<StandaloneEwbDocumentDto?> GetStandaloneEwbDocumentAsync(int dcId, CancellationToken ct)
        {
            const string sql = @"
                -- 1) Header — joins DC + latest non-deleted EWB + From-plant + To-plant + companies
                SELECT TOP 1
                    -- Document classification
                    dc.DeliveryNumber                                         AS DocumentNumber,
                    dc.DeliveryDate                                           AS DocumentDate,
                    eh.SupplyType                                             AS SupplyType,
                    eh.SubSupplyType                                          AS SubSupplyType,
                    eh.TransactionType                                        AS TransactionType,
                    eh.DocumentType                                           AS DocumentType,

                    -- EWB
                    eh.EWBNumber                                              AS EwbNumber,
                    eh.GeneratedDate                                          AS EwbGeneratedDate,
                    eh.ValidUpto                                              AS EwbValidUpto,
                    eh.EwbStatus                                              AS EwbStatus,

                    -- Company (from FROM plant's company); CIN not stored, kept null
                    fromCo.LegalName                                          AS CompanyName,
                    fromCo.GstNumber                                          AS CompanyGstNumber,
                    fromCo.PanNumber                                          AS CompanyPAN,
                    CAST(NULL AS varchar)                                     AS CompanyCIN,
                    fromCoContact.Email                                       AS CompanyEmail,
                    fromCo.Website                                            AS CompanyWebsite,
                    COALESCE(fromCoContact.Phone, fromCoAddr.Phone)           AS CompanyPhone,
                    -- Build the registered-office address by concatenating CompanyAddress fields
                    NULLIF(LTRIM(RTRIM(CONCAT_WS(', ',
                        fromCoAddr.AddressLine1,
                        fromCoAddr.AddressLine2,
                        fromCoCity.CityName,
                        fromCoState.StateName,
                        CAST(NULLIF(fromCoAddr.PinCode, 0) AS varchar(10))
                    ))), '')                                                  AS RegisteredOfficeAddress,

                    -- Consignor (From plant)
                    fromU.UnitName                                            AS ConsignorName,
                    eh.FromGSTIN                                              AS ConsignorGstin,
                    fromAddr.AddressLine1                                     AS ConsignorAddressLine1,
                    fromAddr.AddressLine2                                     AS ConsignorAddressLine2,
                    fromCity.CityName                                         AS ConsignorCity,
                    fromState.StateName                                       AS ConsignorState,
                    TRY_CAST(LEFT(eh.FromGSTIN, 2) AS INT)                    AS ConsignorStateCode,
                    fromAddr.PinCode                                          AS ConsignorPincode,
                    fromAddr.ContactNumber                                    AS ConsignorPhone,

                    -- Consignee (To plant)
                    toU.UnitName                                              AS ConsigneeName,
                    eh.ToGSTIN                                                AS ConsigneeGstin,
                    toAddr.AddressLine1                                       AS ConsigneeAddressLine1,
                    toAddr.AddressLine2                                       AS ConsigneeAddressLine2,
                    toCity.CityName                                           AS ConsigneeCity,
                    toState.StateName                                         AS ConsigneeState,
                    TRY_CAST(LEFT(eh.ToGSTIN, 2) AS INT)                      AS ConsigneeStateCode,
                    toAddr.PinCode                                            AS ConsigneePincode,
                    toAddr.ContactNumber                                      AS ConsigneePhone,

                    -- Transporter
                    eh.TransporterName                                        AS TransporterName,
                    eh.TransporterGSTIN                                       AS TransporterGstin,
                    eh.VehicleNo                                              AS VehicleNumber,
                    eh.VehicleType                                            AS VehicleType,
                    eh.TransportMode                                          AS TransportMode,
                    eh.Distance                                               AS TransportDistance,
                    eh.TransDocNo                                             AS TransDocNo,
                    eh.TransDocDate                                           AS TransDocDate,

                    -- Totals
                    eh.TotalValue                                             AS TotalTaxableValue,
                    eh.CGST                                                   AS CGST,
                    eh.SGST                                                   AS SGST,
                    eh.IGST                                                   AS IGST,
                    eh.Cess                                                   AS Cess,
                    eh.InvoiceValue                                           AS InvoiceTotal,

                    -- Audit
                    dc.CreatedByName                                          AS CreatedByName,
                    dc.CreatedDate                                            AS CreatedDate
                FROM   Sales.DeliveryChallanHeader dc
                LEFT   JOIN Finance.EWaybillHeader eh
                       ON eh.InvoiceNo = dc.DeliveryNumber AND eh.UnitId = dc.FromPlantId AND eh.IsDeleted = 0
                LEFT   JOIN AppData.Unit            fromU         ON fromU.Id        = dc.FromPlantId
                LEFT   JOIN AppData.Company         fromCo        ON fromCo.Id       = fromU.CompanyId
                LEFT   JOIN AppData.UnitAddress     fromAddr      ON fromAddr.UnitId = fromU.Id
                LEFT   JOIN AppData.City            fromCity      ON fromCity.Id     = fromAddr.CityId
                LEFT   JOIN AppData.[State]         fromState     ON fromState.Id    = fromCity.StateId
                LEFT   JOIN AppData.CompanyAddress  fromCoAddr    ON fromCoAddr.CompanyId = fromCo.Id
                LEFT   JOIN AppData.City            fromCoCity    ON fromCoCity.Id   = fromCoAddr.CityId
                LEFT   JOIN AppData.[State]         fromCoState   ON fromCoState.Id  = fromCoAddr.StateId
                LEFT   JOIN AppData.CompanyContact  fromCoContact ON fromCoContact.CompanyId = fromCo.Id
                LEFT   JOIN AppData.Unit          toU       ON toU.Id        = dc.ToPlantId
                LEFT   JOIN AppData.UnitAddress   toAddr    ON toAddr.UnitId = toU.Id
                LEFT   JOIN AppData.City          toCity    ON toCity.Id     = toAddr.CityId
                LEFT   JOIN AppData.[State]       toState   ON toState.Id    = toCity.StateId
                WHERE  dc.Id = @DcId AND dc.IsDeleted = 0
                ORDER BY eh.Id DESC;

                -- 2) Lines — joins EWB Detail + DC Detail (for lot + pack range)
                SELECT  ed.ItemSno          AS Sno,
                        ed.ItemName,
                        ed.HsnNo            AS HsnCode,
                        lm.LotCode          AS LotNumber,
                        dcd.BagCount        AS BagCount,
                        CONCAT(dcd.StartPackNo, '-', dcd.EndPackNo) AS PackRange,
                        ed.Qty              AS Quantity,
                        ed.UOM              AS Uom,
                        dcd.ExMillRate      AS Rate,
                        dcd.NetWeight       AS NetWeight,
                        dcd.GrossWeight     AS GrossWeight,
                        ed.TaxableValue     AS TaxableValue,
                        ed.CGST             AS CgstAmount,
                        ed.SGST             AS SgstAmount,
                        ed.IGST             AS IgstAmount
                FROM    Finance.EWaybillHeader eh
                INNER   JOIN Finance.EWaybillDetail ed ON ed.EWaybillHeaderId = eh.Id AND ed.IsDeleted = 0
                INNER   JOIN Sales.DeliveryChallanHeader dc
                        ON dc.DeliveryNumber = eh.InvoiceNo AND dc.FromPlantId = eh.UnitId AND dc.IsDeleted = 0
                LEFT    JOIN Sales.DeliveryChallanDetail dcd
                        ON dcd.DeliveryChallanHeaderId = dc.Id
                        AND dcd.ItemId = ed.ItemId
                LEFT    JOIN Production.LotMaster lm ON lm.Id = dcd.LotId AND lm.IsDeleted = 0
                WHERE   dc.Id = @DcId AND eh.IsDeleted = 0
                  AND   eh.Id = (SELECT TOP 1 Id FROM Finance.EWaybillHeader
                                 WHERE InvoiceNo = dc.DeliveryNumber
                                   AND UnitId = dc.FromPlantId
                                   AND IsDeleted = 0
                                 ORDER BY Id DESC)
                ORDER BY ed.ItemSno;
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(
                new CommandDefinition(sql, new { DcId = dcId }, cancellationToken: ct));

            var header = await multi.ReadFirstOrDefaultAsync<StandaloneEwbDocumentDto>();
            if (header == null) return null;

            var lines = (await multi.ReadAsync<StandaloneEwbDocumentItemDto>()).ToList();
            header.Items = lines;
            header.TotalQuantity = lines.Sum(l => l.Quantity);
            header.InvoiceTotalInWords = ConvertAmountToWords(header.InvoiceTotal) + " only";

            return header;
        }
    }
}
