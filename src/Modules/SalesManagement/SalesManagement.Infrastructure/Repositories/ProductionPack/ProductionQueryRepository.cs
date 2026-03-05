using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionQueryRepository : IProductionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IItemLookup _itemLookup;

        public ProductionQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _itemLookup = itemLookup;
        }

        public async Task<(List<ProductionPackHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.PackNo LIKE @Search OR h.Remarks LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Production.ProductionPackHeader h
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.PackNo, h.PackDate,
                    h.UnitId, h.WarehouseId,
                    h.TotalBags, h.TotalNetWeight,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Production.ProductionPackHeader h
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ProductionPackHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Populate cross-module header lookup names
                var unitIds = list.Select(x => x.UnitId).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var warehouseIds = list.Select(x => x.WarehouseId).Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var whDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                foreach (var item in list)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.WarehouseName = whDict.TryGetValue(item.WarehouseId, out var wName) ? wName : null;
                }

                // Fetch detail rows for all headers with same-module JOINs
                var headerIds = list.Select(x => x.Id).ToList();
                const string detailSql = @"
                    SELECT d.Id, d.ProductionPackHeaderId, d.ItemSno,
                        d.LotId,
                        lm.LotCode,
                        d.ItemId,
                        d.PackTypeId,
                        pt.PackTypeName,
                        d.NetWeightPerPack,
                        d.StartPackNo, d.EndPackNo,
                        d.TotalBags, d.TotalNetWeight,
                        d.BinId,
                        d.QualityStatusId,
                        qs.Description AS QualityStatusName,
                        d.LineRemarks
                    FROM Production.ProductionPackDetail d
                    LEFT JOIN Sales.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                    LEFT JOIN Sales.PackType pt ON d.PackTypeId = pt.Id AND pt.IsDeleted = 0
                    LEFT JOIN Sales.MiscMaster qs ON d.QualityStatusId = qs.Id AND qs.IsDeleted = 0
                    WHERE d.ProductionPackHeaderId IN @HeaderIds";

                var allDetails = (await _dbConnection.QueryAsync<ProductionPackDetailDto>(
                    detailSql, new { HeaderIds = headerIds })).ToList();

                // Populate cross-module detail lookup names (ItemName, BinName)
                if (allDetails.Count > 0)
                {
                    var itemIds = allDetails.Select(d => d.ItemId).Distinct();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                    var binIds = allDetails.Select(d => d.BinId).Distinct();
                    var bins = await _binLookup.GetByIdsAsync(binIds);
                    var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

                    foreach (var detail in allDetails)
                    {
                        detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                        detail.BinName = binDict.TryGetValue(detail.BinId, out var bName) ? bName : null;
                    }
                }

                // Group details by header and assign
                var detailsByHeader = allDetails.GroupBy(d => d.ProductionPackHeaderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in list)
                {
                    item.ProductionPackDetails = detailsByHeader.TryGetValue(item.Id, out var details)
                        ? details
                        : new List<ProductionPackDetailDto>();
                }
            }

            return (list, totalCount);
        }

        public async Task<ProductionPackHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.PackNo, h.PackDate,
                    h.UnitId, h.WarehouseId,
                    h.TotalBags, h.TotalNetWeight,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Production.ProductionPackHeader h
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<ProductionPackHeaderDto>(
                headerSql, new { Id = id });

            if (header == null)
                return null;

            // Fetch detail rows with same-module JOINs
            const string detailSql = @"
                SELECT d.Id, d.ProductionPackHeaderId, d.ItemSno,
                    d.LotId,
                    lm.LotCode,
                    d.ItemId,
                    d.PackTypeId,
                    pt.PackTypeName,
                    d.NetWeightPerPack,
                    d.StartPackNo, d.EndPackNo,
                    d.TotalBags, d.TotalNetWeight,
                    d.BinId,
                    d.QualityStatusId,
                    qs.Description AS QualityStatusName,
                    d.LineRemarks
                FROM Production.ProductionPackDetail d
                LEFT JOIN Sales.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Sales.PackType pt ON d.PackTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qs ON d.QualityStatusId = qs.Id AND qs.IsDeleted = 0
                WHERE d.ProductionPackHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<ProductionPackDetailDto>(
                detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module header lookups
            var unitLookup = await _unitLookup.GetByIdAsync(header.UnitId);
            header.UnitName = unitLookup?.UnitName;

            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { header.WarehouseId });
            header.WarehouseName = warehouses.FirstOrDefault()?.WarehouseName;

            // Populate cross-module detail lookups (ItemName, BinName)
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var binIds = details.Select(d => d.BinId).Distinct();
                var bins = await _binLookup.GetByIdsAsync(binIds);
                var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    detail.BinName = binDict.TryGetValue(detail.BinId, out var bName) ? bName : null;
                }
            }

            header.ProductionPackDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<ProductionLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, PackNo, PackDate
                FROM Production.ProductionPackHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                    AND PackNo LIKE @Search
                ORDER BY PackNo";

            var result = await _dbConnection.QueryAsync<ProductionLookupDto>(
                sql, new { Search = $"%{term}%" });

            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.ProductionPackHeader
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        // ── FK validation methods ────────────────────────────────────────────

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var unit = await _unitLookup.GetByIdAsync(unitId);
            return unit != null;
        }

        public async Task<bool> WarehouseExistsAsync(int warehouseId)
        {
            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { warehouseId });
            return warehouses.Any();
        }

        public async Task<bool> BinExistsAsync(int binId)
        {
            var bins = await _binLookup.GetByIdsAsync(new[] { binId });
            return bins.Any();
        }

        public async Task<bool> BinBelongsToWarehouseAsync(int binId, int warehouseId)
        {
            var bins = await _binLookup.GetByWarehouseIdAsync(warehouseId);
            return bins.Any(b => b.Id == binId);
        }

        public async Task<bool> LotExistsAsync(int lotId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.LotMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = lotId });
        }

        public async Task<bool> PackTypeExistsAsync(int packTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.PackType
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = packTypeId });
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<bool> QualityStatusExistsAsync(int qualityStatusId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.MiscMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = qualityStatusId });
        }

        public async Task<bool> PackOverlapExistsAsync(
            int lotId, int startPackNo, int endPackNo, int? excludeDetailId = null)
        {
            var excludeFilter = excludeDetailId.HasValue
                ? "AND d.Id != @ExcludeDetailId"
                : "";

            var sql = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.ProductionPackDetail d
                    INNER JOIN Production.ProductionPackHeader h ON d.ProductionPackHeaderId = h.Id
                    WHERE d.LotId = @LotId
                        AND d.PackTypeId = @PackTypeId
                        AND d.StartPackNo <= @EndPackNo
                        AND d.EndPackNo >= @StartPackNo
                        AND h.IsDeleted = 0
                        {excludeFilter}
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                LotId = lotId,
                //PackTypeId = packTypeId,
                StartPackNo = startPackNo,
                EndPackNo = endPackNo,
                ExcludeDetailId = excludeDetailId
            });
        }
    }
}
