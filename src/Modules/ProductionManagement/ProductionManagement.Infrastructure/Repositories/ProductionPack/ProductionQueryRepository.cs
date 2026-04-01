using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionQueryRepository : IProductionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ISalesStockLedgerService _stockLedgerLookup;

        public ProductionQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IItemLookup itemLookup,
            IIPAddressService ipAddressService,
            ISalesStockLedgerService stockLedgerLookup)
        {
            _dbConnection      = dbConnection;
            _unitLookup        = unitLookup;
            _warehouseLookup   = warehouseLookup;
            _binLookup         = binLookup;
            _itemLookup        = itemLookup;
            _ipAddressService  = ipAddressService;
            _stockLedgerLookup = stockLedgerLookup;
        }

        public async Task<(List<ProductionPackHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId();

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.PackNo LIKE @Search OR h.Remarks LIKE @Search)";

            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Production.ProductionPackHeader h
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter};

                SELECT h.Id, h.PackNo, h.PackDate, h.ProductionYear,
                    h.UnitId, h.WarehouseId,
                    h.TotalBags, h.TotalNetWeight,
                    h.ProductionKgs, h.LooseConeKgs,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Production.ProductionPackHeader h
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                UnitId = unitId,
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

            }

            return (list, totalCount);
        }

        public async Task<ProductionPackHeaderDto?> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var headerSql = $@"
                SELECT h.Id, h.PackNo, h.PackDate, h.ProductionYear,
                    h.UnitId, h.WarehouseId,
                    h.TotalBags, h.TotalNetWeight,
                    h.ProductionKgs, h.LooseConeKgs,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Production.ProductionPackHeader h
                WHERE h.Id = @Id AND h.IsDeleted = 0 {unitFilter}";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<ProductionPackHeaderDto>(
                headerSql, new { Id = id, UnitId = unitId });

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
                    d.NoOfBags,
                    d.TotalBags, d.TotalNetWeight,
                    d.BinId,
                    d.QualityStatusId,
                    qs.Description AS QualityStatusName,
                    d.LineRemarks
                FROM Production.ProductionPackDetail d
                LEFT JOIN Production.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.PackType pt ON d.PackTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster qs ON d.QualityStatusId = qs.Id AND qs.IsDeleted = 0
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
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT Id, PackNo, PackDate
                FROM Production.ProductionPackHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                    AND PackNo LIKE @Search
                    {unitFilter}
                ORDER BY PackNo";

            var result = await _dbConnection.QueryAsync<ProductionLookupDto>(
                sql, new { Search = $"%{term}%", UnitId = unitId });

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
                    SELECT 1 FROM Production.LotMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = lotId });
        }

        public async Task<bool> PackTypeExistsAsync(int packTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.PackType
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
                    SELECT 1 FROM Production.MiscMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = qualityStatusId });
        }

        public async Task<int> GetLastEndPackNoAsync(int productionYear)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            return await _stockLedgerLookup.GetLastPackNoByYearAsync(productionYear, unitId);
        }

        public async Task<List<ProductionPackDetailDto>> GetByPackRangeAsync(
            int startPackNo, int endPackNo, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var sql = $@"
                SELECT d.Id, d.ProductionPackHeaderId,
                    h.PackNo, h.PackDate, h.ProductionYear,
                    h.UnitId, h.WarehouseId,
                    d.ItemSno,
                    d.LotId, lm.LotCode,
                    d.ItemId,
                    d.PackTypeId, pt.PackTypeName,
                    d.NetWeightPerPack,
                    d.StartPackNo, d.EndPackNo,
                    d.NoOfBags,
                    d.TotalBags, d.TotalNetWeight,
                    d.BinId,
                    d.QualityStatusId,
                    qs.Description AS QualityStatusName,
                    d.LineRemarks
                FROM Production.ProductionPackDetail d
                INNER JOIN Production.ProductionPackHeader h
                    ON d.ProductionPackHeaderId = h.Id AND h.IsDeleted = 0
                    {unitFilter}
                LEFT JOIN Production.LotMaster  lm ON d.LotId          = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.PackType   pt ON d.PackTypeId      = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster qs ON d.QualityStatusId = qs.Id AND qs.IsDeleted = 0
                WHERE d.StartPackNo = @StartPackNo
                  AND d.EndPackNo   = @EndPackNo
                ORDER BY d.StartPackNo;";

            var details = (await _dbConnection.QueryAsync<ProductionPackDetailDto>(
                sql, new { StartPackNo = startPackNo, EndPackNo = endPackNo, UnitId = unitId })).ToList();

            if (details.Count == 0)
                return details;

            // Filter: only keep details that have at least one 'Packed' pack in Sales.StockLedger
            var productionYear = details[0].ProductionYear ?? 0;
            var resolvedUnitId = unitId ?? 0;
            var packedNos = await _stockLedgerLookup.GetPackedPackNosAsync(startPackNo, endPackNo, productionYear, resolvedUnitId, ct);
            var packedSet = new HashSet<int>(packedNos);
            details = details.Where(d => packedSet.Contains(d.StartPackNo)).ToList();

            if (details.Count > 0)
            {
                var itemIds  = details.Select(d => d.ItemId).Distinct();
                var items    = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var binIds  = details.Select(d => d.BinId).Distinct();
                var bins    = await _binLookup.GetByIdsAsync(binIds);
                var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

                var unitIds    = details.Where(d => d.UnitId.HasValue).Select(d => d.UnitId!.Value).Distinct();
                var units      = await _unitLookup.GetByIdsAsync(unitIds, ct);
                var unitDict   = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var whIds      = details.Where(d => d.WarehouseId.HasValue).Select(d => d.WarehouseId!.Value).Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(whIds, ct);
                var whDict     = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                foreach (var detail in details)
                {
                    detail.ItemName      = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    detail.BinName       = binDict.TryGetValue(detail.BinId,   out var bName) ? bName : null;
                    detail.UnitName      = detail.UnitId.HasValue && unitDict.TryGetValue(detail.UnitId.Value,      out var uName) ? uName : null;
                    detail.WarehouseName = detail.WarehouseId.HasValue && whDict.TryGetValue(detail.WarehouseId.Value, out var wName) ? wName : null;
                }
            }

            return details;
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
                        AND d.StartPackNo <= @EndPackNo
                        AND d.EndPackNo >= @StartPackNo
                        AND h.IsDeleted = 0
                        {excludeFilter}
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                LotId = lotId,
                StartPackNo = startPackNo,
                EndPackNo = endPackNo,
                ExcludeDetailId = excludeDetailId
            });
        }
    }
}
