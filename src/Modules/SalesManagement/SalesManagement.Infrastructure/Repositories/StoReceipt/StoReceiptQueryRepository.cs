using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Infrastructure.Repositories.StoReceipt
{
    public class StoReceiptQueryRepository : IStoReceiptQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IRackLookup _rackLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;

        public StoReceiptQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IRackLookup rackLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _rackLookup = rackLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
        }

        public async Task<(List<StoReceiptHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.StoReceiptNumber LIKE @SearchTerm
                       OR dc.DeliveryNumber LIKE @SearchTerm
                       OR ms.Description LIKE @SearchTerm
                       OR h.VehicleNumber LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.StoReceiptHeader h
                LEFT JOIN Sales.DeliveryChallanHeader dc ON h.DeliveryChallanHeaderId = dc.Id AND dc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.StoReceiptNumber,
                    h.StoReceiptDate,
                    h.DeliveryChallanHeaderId,
                    dc.DeliveryNumber,
                    h.ReceivingPlantId,
                    h.ReceivingStorageLocationId,
                    h.RackId,
                    h.VehicleNumber,
                    h.Remarks,
                    h.StatusId,
                    ms.Description AS StatusName,
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
                FROM Sales.StoReceiptHeader h
                LEFT JOIN Sales.DeliveryChallanHeader dc ON h.DeliveryChallanHeaderId = dc.Id AND dc.IsDeleted = 0
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
            var data = (await multi.ReadAsync<StoReceiptHeaderDto>()).ToList();

            // Populate cross-module FK names via lookups
            if (data.Count > 0)
            {
                var plantIds = data.Select(d => d.ReceivingPlantId).Distinct();
                var plants = await _unitLookup.GetByIdsAsync(plantIds);
                var plantDict = plants.ToDictionary(p => p.UnitId, p => p.UnitName);

                var warehouseIds = data.Select(d => d.ReceivingStorageLocationId).Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                var rackIds = data.Where(d => d.RackId.HasValue).Select(d => d.RackId!.Value).Distinct();
                var racks = await _rackLookup.GetByIdsAsync(rackIds);
                var rackDict = racks.ToDictionary(r => r.Id, r => r.RackName);

                foreach (var item in data)
                {
                    item.ReceivingPlantName = plantDict.TryGetValue(item.ReceivingPlantId, out var pName) ? pName : null;
                    item.ReceivingStorageLocationName = warehouseDict.TryGetValue(item.ReceivingStorageLocationId, out var wName) ? wName : null;
                    item.RackName = item.RackId.HasValue && rackDict.TryGetValue(item.RackId.Value, out var rName) ? rName : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<StoReceiptHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id,
                    h.StoReceiptNumber,
                    h.StoReceiptDate,
                    h.DeliveryChallanHeaderId,
                    dc.DeliveryNumber,
                    h.ReceivingPlantId,
                    h.ReceivingStorageLocationId,
                    h.RackId,
                    h.VehicleNumber,
                    h.Remarks,
                    h.StatusId,
                    ms.Description AS StatusName,
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
                FROM Sales.StoReceiptHeader h
                LEFT JOIN Sales.DeliveryChallanHeader dc ON h.DeliveryChallanHeaderId = dc.Id AND dc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<StoReceiptHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Populate cross-module header lookups
            var plant = await _unitLookup.GetByIdAsync(header.ReceivingPlantId);
            header.ReceivingPlantName = plant?.UnitName;

            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { header.ReceivingStorageLocationId });
            var wh = warehouses.FirstOrDefault();
            header.ReceivingStorageLocationName = wh?.WarehouseName;

            if (header.RackId.HasValue)
            {
                var rackList = await _rackLookup.GetByIdsAsync(new[] { header.RackId.Value });
                header.RackName = rackList.FirstOrDefault()?.RackName;
            }

            // Fetch details with Lot + LineStatus JOINs
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.StoReceiptHeaderId,
                    d.DeliveryChallanDetailId,
                    d.ItemId,
                    d.LotId,
                    lm.LotCode,
                    d.StartPackNo,
                    d.EndPackNo,
                    d.DispatchQuantity,
                    d.ReceivedQuantity,
                    d.DamageQuantity,
                    d.AcceptedQuantity,
                    d.UOMId,
                    d.BagCount,
                    d.NetWeight,
                    d.GrossWeight,
                    d.LineStatusId,
                    ms2.Description AS LineStatusName
                FROM Sales.StoReceiptDetail d
                LEFT JOIN Sales.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms2 ON d.LineStatusId = ms2.Id AND ms2.IsDeleted = 0
                WHERE d.StoReceiptHeaderId = @HeaderId;";

            var details = (await _dbConnection.QueryAsync<StoReceiptDetailDto>(detailSql, new { HeaderId = id })).ToList();

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

            header.StoReceiptDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<StoReceiptLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    h.Id,
                    h.StoReceiptNumber,
                    h.StoReceiptDate,
                    dc.DeliveryNumber
                FROM Sales.StoReceiptHeader h
                LEFT JOIN Sales.DeliveryChallanHeader dc ON h.DeliveryChallanHeaderId = dc.Id AND dc.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                  AND (h.StoReceiptNumber LIKE @Term OR dc.DeliveryNumber LIKE @Term)
                ORDER BY h.StoReceiptNumber ASC;";

            var result = await _dbConnection.QueryAsync<StoReceiptLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoReceiptHeader
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> DeliveryChallanHeaderExistsAsync(int dcHeaderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DeliveryChallanHeader
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dcHeaderId });
            return count > 0;
        }

        public async Task<DcOpenQtyDto?> GetDcOpenQtyAsync(int dcDetailId)
        {
            const string sql = @"
                SELECT
                    dcd.Id AS DeliveryChallanDetailId,
                    dcd.ItemId,
                    dcd.LotId,
                    dcd.DispatchQuantity,
                    ISNULL(SUM(srd.ReceivedQuantity), 0) AS AlreadyReceivedQuantity,
                    dcd.DispatchQuantity - ISNULL(SUM(srd.ReceivedQuantity), 0) AS OpenQuantity,
                    dcd.UOMId,
                    dcd.StartPackNo,
                    dcd.EndPackNo
                FROM Sales.DeliveryChallanDetail dcd
                LEFT JOIN Sales.StoReceiptDetail srd ON dcd.Id = srd.DeliveryChallanDetailId
                    AND srd.StoReceiptHeaderId IN (
                        SELECT Id FROM Sales.StoReceiptHeader WHERE IsDeleted = 0
                    )
                WHERE dcd.Id = @DcDetailId
                GROUP BY dcd.Id, dcd.ItemId, dcd.LotId, dcd.DispatchQuantity, dcd.UOMId, dcd.StartPackNo, dcd.EndPackNo;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<DcOpenQtyDto>(sql, new { DcDetailId = dcDetailId });

            if (result != null)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { result.ItemId });
                var item = items.FirstOrDefault();
                result.ItemName = item?.ItemName;

                var uoms = await _uomLookup.GetByIdsAsync(new[] { result.UOMId });
                var uom = uoms.FirstOrDefault();
                result.UOMName = uom?.UOMName;

                // Fetch LotCode via same-module query
                const string lotSql = @"
                    SELECT LotCode FROM Sales.LotMaster WHERE Id = @LotId AND IsDeleted = 0;";
                result.LotCode = await _dbConnection.ExecuteScalarAsync<string>(lotSql, new { result.LotId });
            }

            return result;
        }
    }
}
