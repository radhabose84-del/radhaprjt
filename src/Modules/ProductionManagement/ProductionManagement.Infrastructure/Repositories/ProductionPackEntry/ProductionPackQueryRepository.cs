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
                FROM Production.ProductionPackEntry h
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter};

                SELECT h.Id, h.PackNo, h.PackDate, h.ProductionYear,
                    h.UnitId, h.WarehouseId,
                    h.ItemId, h.VariantId, h.LotId, h.PackTypeId, h.NetWeightPerPack,
                    h.StartPackNo, h.EndPackNo,
                    h.OpeningLooseKgs, h.TotalProductionKgs,
                    h.TotalBags, h.TotalNetWeight,
                    h.ProductionKgs, h.LooseConeKgs,
                    h.BinId, h.QualityStatusId,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName,
                    lm.LotCode,
                    pt.PackTypeName
                FROM Production.ProductionPackEntry h
                LEFT JOIN Production.LotMaster lm ON h.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.PackType   pt ON h.PackTypeId = pt.Id AND pt.IsDeleted = 0
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                UnitId   = unitId,
                Search   = $"%{searchTerm}%",
                Offset   = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result     = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list       = (await result.ReadAsync<ProductionPackHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var unitIds  = list.Select(x => x.UnitId).Distinct();
                var units    = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var warehouseIds = list.Select(x => x.WarehouseId).Distinct();
                var warehouses   = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var whDict       = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                var itemIds  = list.Select(x => x.ItemId).Distinct();
                var items    = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var variantIds = list.Where(x => x.VariantId.HasValue).Select(x => x.VariantId!.Value).Distinct();
                var variantDict = variantIds.Any()
                    ? (await _itemLookup.GetByIdsAsync(variantIds)).ToDictionary(i => i.Id, i => (string?)i.ItemName)
                    : new Dictionary<int, string?>();

                var binIds  = list.Where(x => x.BinId.HasValue).Select(x => x.BinId!.Value).Distinct();
                var bins    = binIds.Any() ? await _binLookup.GetByIdsAsync(binIds) : [];
                var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

                foreach (var item in list)
                {
                    item.UnitName      = unitDict.TryGetValue(item.UnitId,    out var uName) ? uName : null;
                    item.WarehouseName = whDict.TryGetValue(item.WarehouseId, out var wName) ? wName : null;
                    item.ItemName      = itemDict.TryGetValue(item.ItemId,    out var iName) ? iName : null;
                    item.VariantName   = item.VariantId.HasValue && variantDict.TryGetValue(item.VariantId.Value, out var vName) ? vName : null;
                    item.BinName       = item.BinId.HasValue && binDict.TryGetValue(item.BinId.Value, out var bName) ? bName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<ProductionPackHeaderDto?> GetByIdAsync(int id)
        {
            var unitId     = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var sql = $@"
                SELECT h.Id, h.PackNo, h.PackDate, h.ProductionYear,
                    h.UnitId, h.WarehouseId,
                    h.ItemId, h.VariantId, h.LotId, h.PackTypeId, h.NetWeightPerPack,
                    h.StartPackNo, h.EndPackNo,
                    h.OpeningLooseKgs, h.TotalProductionKgs,
                    h.TotalBags, h.TotalNetWeight,
                    h.ProductionKgs, h.LooseConeKgs,
                    h.BinId, h.QualityStatusId,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName,
                    lm.LotCode,
                    pt.PackTypeName,
                    qs.Description AS QualityStatusName
                FROM Production.ProductionPackEntry h
                LEFT JOIN Production.LotMaster  lm ON h.LotId           = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.PackType    pt ON h.PackTypeId       = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster  qs ON h.QualityStatusId  = qs.Id AND qs.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0 {unitFilter}";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ProductionPackHeaderDto>(
                sql, new { Id = id, UnitId = unitId });

            if (dto == null)
                return null;

            var unitLookup = await _unitLookup.GetByIdAsync(dto.UnitId);
            dto.UnitName = unitLookup?.UnitName;

            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { dto.WarehouseId });
            dto.WarehouseName = warehouses.FirstOrDefault()?.WarehouseName;

            var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId });
            dto.ItemName = items.FirstOrDefault()?.ItemName;

            if (dto.VariantId.HasValue)
            {
                var variants = await _itemLookup.GetByIdsAsync(new[] { dto.VariantId.Value });
                dto.VariantName = variants.FirstOrDefault()?.ItemName;
            }

            if (dto.BinId.HasValue)
            {
                var bins = await _binLookup.GetByIdsAsync(new[] { dto.BinId.Value });
                dto.BinName = bins.FirstOrDefault()?.BinName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<ProductionLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            var unitId     = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT Id, PackNo, PackDate
                FROM Production.ProductionPackEntry
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
                    SELECT 1 FROM Production.ProductionPackEntry
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

        public async Task<bool> PackOverlapExistsAsync(
            int lotId, int startPackNo, int endPackNo, int? excludeId = null)
        {
            var excludeFilter = excludeId.HasValue ? "AND h.Id != @ExcludeId" : "";

            var sql = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.ProductionPackEntry h
                    WHERE h.LotId = @LotId
                        AND h.StartPackNo <= @EndPackNo
                        AND h.EndPackNo >= @StartPackNo
                        AND h.IsDeleted = 0
                        {excludeFilter}
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                LotId       = lotId,
                StartPackNo = startPackNo,
                EndPackNo   = endPackNo,
                ExcludeId   = excludeId
            });
        }

        public async Task<ProductionStockClosingDto?> GetPreviousDateClosingAsync(
            int itemId, int lotId, DateOnly docDate)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT TOP 1
                    ClosingLooseKgs,
                    ClosingPackKgs,
                    ClosingBags
                FROM Production.ProductionStockLedger
                WHERE UnitId = @UnitId AND ItemId = @ItemId AND LotId = @LotId
                    AND DocDate <= @DocDate
                ORDER BY DocDate DESC, Id DESC";

            return await _dbConnection.QueryFirstOrDefaultAsync<ProductionStockClosingDto>(
                sql, new { UnitId = unitId, ItemId = itemId, LotId = lotId, DocDate = docDate.ToDateTime(TimeOnly.MinValue) });
        }

        public async Task<List<ProductionStockRegisterDto>> GetProductionStockRegisterAsync(
            DateOnly fromDate, DateOnly toDate, int? lotId, int? itemId)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND sl.UnitId = @UnitId" : "";
            var lotFilter = lotId.HasValue ? "AND sl.LotId = @LotId" : "";
            var itemFilter = itemId.HasValue ? "AND sl.ItemId = @ItemId" : "";

            var sql = $@"
                SELECT sl.Id, sl.UnitId, sl.ItemId, sl.LotId,
                    sl.DocDate,
                    sl.OpeningLooseKgs, sl.ProdKgs, sl.TotalProdKgs,
                    sl.PackTypeId, sl.NetWeightPerPack,
                    sl.TotalBags, sl.NetWeight,
                    sl.BagsRepacked, sl.RepackKgs,
                    sl.ClosingLooseKgs, sl.ClosingPackKgs, sl.ClosingBags,
                    sl.StockClosing,
                    lm.LotCode,
                    pt.PackTypeName
                FROM Production.ProductionStockLedger sl
                LEFT JOIN Production.LotMaster lm ON sl.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.PackType pt ON sl.PackTypeId = pt.Id AND pt.IsDeleted = 0
                WHERE sl.DocDate >= @FromDate AND sl.DocDate <= @ToDate
                    {unitFilter} {lotFilter} {itemFilter}
                ORDER BY sl.ItemId, sl.LotId, sl.DocDate, sl.Id";

            var parameters = new
            {
                FromDate = fromDate.ToDateTime(TimeOnly.MinValue),
                ToDate   = toDate.ToDateTime(TimeOnly.MinValue),
                UnitId   = unitId,
                LotId    = lotId,
                ItemId   = itemId
            };

            var list = (await _dbConnection.QueryAsync<ProductionStockRegisterDto>(sql, parameters)).ToList();

            if (list.Count > 0)
            {
                var unitIds = list.Select(x => x.UnitId).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var itemIds = list.Select(x => x.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                foreach (var item in list)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.ItemName = itemDict.TryGetValue(item.ItemId, out var iName) ? iName : null;
                }
            }

            return list;
        }

        public async Task<DateOnly?> GetLastStockLedgerDateAsync()
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT TOP 1 DocDate
                FROM Production.ProductionStockLedger
                WHERE 1 = 1 {unitFilter}
                ORDER BY DocDate DESC";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<DateTime?>(
                sql, new { UnitId = unitId });

            return result.HasValue ? DateOnly.FromDateTime(result.Value) : null;
        }

    }
}
