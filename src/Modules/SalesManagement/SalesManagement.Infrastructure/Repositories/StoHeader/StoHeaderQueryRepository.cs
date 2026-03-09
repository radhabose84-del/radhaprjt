using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Infrastructure.Repositories.StoHeader
{
    public class StoHeaderQueryRepository : IStoHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;

        public StoHeaderQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
        }

        public async Task<(List<StoHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.StoNumber LIKE @SearchTerm
                       OR st.StoTypeCode LIKE @SearchTerm
                       OR st.StoTypeName LIKE @SearchTerm
                       OR mt.MovementCode LIKE @SearchTerm
                       OR mt.MovementDescription LIKE @SearchTerm
                       OR hs.Description LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.StoHeader h
                LEFT JOIN Sales.StoTypeMaster st ON h.StoTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster hs ON h.HeaderStatusId = hs.Id AND hs.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.StoNumber,
                    h.DocumentDate,
                    h.ExpectedDeliveryDate,
                    h.StoTypeId,
                    st.StoTypeCode,
                    st.StoTypeName,
                    h.MovementTypeId,
                    mt.MovementCode,
                    mt.MovementDescription,
                    h.SupplyingPlantId,
                    h.SupplyingStorageLocationId,
                    h.ReceivingPlantId,
                    h.ReceivingStorageLocationId,
                    h.Remarks,
                    h.HeaderStatusId,
                    hs.Description AS HeaderStatusName,
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
                FROM Sales.StoHeader h
                LEFT JOIN Sales.StoTypeMaster st ON h.StoTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster hs ON h.HeaderStatusId = hs.Id AND hs.IsDeleted = 0
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
            var data = (await multi.ReadAsync<StoHeaderDto>()).ToList();

            // Populate cross-module FK names via lookups
            if (data.Count > 0)
            {
                var supplyingPlantIds = data.Select(d => d.SupplyingPlantId).Distinct();
                var receivingPlantIds = data.Select(d => d.ReceivingPlantId).Distinct();
                var allPlantIds = supplyingPlantIds.Union(receivingPlantIds).Distinct();
                var plants = await _unitLookup.GetByIdsAsync(allPlantIds);
                var plantDict = plants.ToDictionary(p => p.UnitId, p => p.UnitName);

                var warehouseIds = data.Select(d => d.SupplyingStorageLocationId)
                    .Union(data.Select(d => d.ReceivingStorageLocationId))
                    .Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                foreach (var item in data)
                {
                    item.SupplyingPlantName = plantDict.TryGetValue(item.SupplyingPlantId, out var spName) ? spName : null;
                    item.ReceivingPlantName = plantDict.TryGetValue(item.ReceivingPlantId, out var rpName) ? rpName : null;
                    item.SupplyingStorageLocationName = warehouseDict.TryGetValue(item.SupplyingStorageLocationId, out var ssName) ? ssName : null;
                    item.ReceivingStorageLocationName = warehouseDict.TryGetValue(item.ReceivingStorageLocationId, out var rsName) ? rsName : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<StoHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id,
                    h.StoNumber,
                    h.DocumentDate,
                    h.ExpectedDeliveryDate,
                    h.StoTypeId,
                    st.StoTypeCode,
                    st.StoTypeName,
                    h.MovementTypeId,
                    mt.MovementCode,
                    mt.MovementDescription,
                    h.SupplyingPlantId,
                    h.SupplyingStorageLocationId,
                    h.ReceivingPlantId,
                    h.ReceivingStorageLocationId,
                    h.Remarks,
                    h.HeaderStatusId,
                    hs.Description AS HeaderStatusName,
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
                FROM Sales.StoHeader h
                LEFT JOIN Sales.StoTypeMaster st ON h.StoTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig mt ON h.MovementTypeId = mt.Id AND mt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster hs ON h.HeaderStatusId = hs.Id AND hs.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<StoHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Populate cross-module header lookups
            var supplyingPlant = await _unitLookup.GetByIdAsync(header.SupplyingPlantId);
            header.SupplyingPlantName = supplyingPlant?.UnitName;

            var receivingPlant = await _unitLookup.GetByIdAsync(header.ReceivingPlantId);
            header.ReceivingPlantName = receivingPlant?.UnitName;

            var warehouses = await _warehouseLookup.GetByIdsAsync(
                new[] { header.SupplyingStorageLocationId, header.ReceivingStorageLocationId });
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);
            header.SupplyingStorageLocationName = warehouseDict.TryGetValue(header.SupplyingStorageLocationId, out var ssName) ? ssName : null;
            header.ReceivingStorageLocationName = warehouseDict.TryGetValue(header.ReceivingStorageLocationId, out var rsName) ? rsName : null;

            // Fetch details with LineStatus JOIN
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.StoHeaderId,
                    d.ItemId,
                    d.Quantity,
                    d.UOMId,
                    d.TransferPrice,
                    d.LineStatusId,
                    mm.Description AS LineStatusName
                FROM Sales.StoDetail d
                LEFT JOIN Sales.MiscMaster mm ON d.LineStatusId = mm.Id AND mm.IsDeleted = 0
                WHERE d.StoHeaderId = @HeaderId;";

            var details = (await _dbConnection.QueryAsync<StoDetailDto>(detailSql, new { HeaderId = id })).ToList();

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

            header.StoDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<StoHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    h.Id,
                    h.StoNumber,
                    st.StoTypeName
                FROM Sales.StoHeader h
                LEFT JOIN Sales.StoTypeMaster st ON h.StoTypeId = st.Id AND st.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                  AND (h.StoNumber LIKE @Term OR st.StoTypeName LIKE @Term)
                ORDER BY h.StoNumber ASC;";

            var result = await _dbConnection.QueryAsync<StoHeaderLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoHeader
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> StoTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoTypeMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> MovementTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MovementTypeConfig
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
