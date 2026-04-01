using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;

namespace ProductionManagement.Infrastructure.Repositories.YarnConversionHeader
{
    public class YarnConversionHeaderQueryRepository : IYarnConversionHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public YarnConversionHeaderQueryRepository(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IUnitLookup unitLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _unitLookup = unitLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
        }

        public async Task<(List<YarnConversionHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.YarnConversionHeader yc
                WHERE yc.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR yc.ConversionDocNo LIKE @SearchTerm);

                SELECT
                    yc.Id, yc.UnitId, yc.ProductionYear, yc.ConversionDocNo, yc.ConversionDate,
                    yc.LotId,
                    lm.LotCode AS LotName,
                    yc.OldItemId, yc.OldPackTypeId,
                    opt.PackTypeName AS OldPackTypeName,
                    yc.OldStartPackNo, yc.OldEndPackNo, yc.OldTotalBags,
                    yc.OldNetWeightPerPack, yc.OldNetWeight,
                    yc.OldWarehouseId, yc.OldBinId,
                    yc.FaultId,
                    fm.Description AS FaultName,
                    yc.ItemId, yc.PackTypeId,
                    npt.PackTypeName AS PackTypeName,
                    yc.TotalBags, yc.NetWeightPerPack, yc.NetWeight,
                    yc.StartPackNo, yc.EndPackNo,
                    yc.LooseQty, yc.LooseHandlingId,
                    lh.Description AS LooseHandlingName,
                    yc.WarehouseId, yc.BinId,
                    yc.WasteTypeId,
                    wt.Description AS WasteTypeName,
                    yc.WasteQty, yc.WasteReason,
                    yc.Remarks,
                    yc.IsActive, yc.IsDeleted,
                    yc.CreatedBy, yc.CreatedDate, yc.CreatedByName,
                    yc.ModifiedBy, yc.ModifiedDate, yc.ModifiedByName
                FROM Production.YarnConversionHeader yc
                LEFT JOIN Production.LotMaster lm  ON yc.LotId          = lm.Id  AND lm.IsDeleted  = 0
                LEFT JOIN Production.PackType opt   ON yc.OldPackTypeId  = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.PackType npt   ON yc.PackTypeId     = npt.Id AND npt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster fm  ON yc.FaultId        = fm.Id  AND fm.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster lh  ON yc.LooseHandlingId= lh.Id  AND lh.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster wt  ON yc.WasteTypeId    = wt.Id  AND wt.IsDeleted  = 0
                WHERE yc.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR yc.ConversionDocNo LIKE @SearchTerm)
                ORDER BY yc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<YarnConversionHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
                await PopulateCrossModuleNamesAsync(list);

            return (list, totalCount);
        }

        public async Task<YarnConversionHeaderDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    yc.Id, yc.UnitId, yc.ProductionYear, yc.ConversionDocNo, yc.ConversionDate,
                    yc.LotId,
                    lm.LotCode AS LotName,
                    yc.OldItemId, yc.OldPackTypeId,
                    opt.PackTypeName AS OldPackTypeName,
                    yc.OldStartPackNo, yc.OldEndPackNo, yc.OldTotalBags,
                    yc.OldNetWeightPerPack, yc.OldNetWeight,
                    yc.OldWarehouseId, yc.OldBinId,
                    yc.FaultId,
                    fm.Description AS FaultName,
                    yc.ItemId, yc.PackTypeId,
                    npt.PackTypeName AS PackTypeName,
                    yc.TotalBags, yc.NetWeightPerPack, yc.NetWeight,
                    yc.StartPackNo, yc.EndPackNo,
                    yc.LooseQty, yc.LooseHandlingId,
                    lh.Description AS LooseHandlingName,
                    yc.WarehouseId, yc.BinId,
                    yc.WasteTypeId,
                    wt.Description AS WasteTypeName,
                    yc.WasteQty, yc.WasteReason,
                    yc.Remarks,
                    yc.IsActive, yc.IsDeleted,
                    yc.CreatedBy, yc.CreatedDate, yc.CreatedByName,
                    yc.ModifiedBy, yc.ModifiedDate, yc.ModifiedByName
                FROM Production.YarnConversionHeader yc
                LEFT JOIN Production.LotMaster lm  ON yc.LotId          = lm.Id  AND lm.IsDeleted  = 0
                LEFT JOIN Production.PackType opt   ON yc.OldPackTypeId  = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.PackType npt   ON yc.PackTypeId     = npt.Id AND npt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster fm  ON yc.FaultId        = fm.Id  AND fm.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster lh  ON yc.LooseHandlingId= lh.Id  AND lh.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster wt  ON yc.WasteTypeId    = wt.Id  AND wt.IsDeleted  = 0
                WHERE yc.Id = @Id AND yc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<YarnConversionHeaderDto>(sql, new { Id = id });

            if (dto != null)
                await PopulateCrossModuleNamesAsync(new List<YarnConversionHeaderDto> { dto });

            return dto;
        }

        public async Task<IReadOnlyList<YarnConversionHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ConversionDocNo, ConversionDate
                FROM Production.YarnConversionHeader
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND ConversionDocNo LIKE @Term
                ORDER BY ConversionDocNo ASC";

            var result = await _dbConnection.QueryAsync<YarnConversionHeaderLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnConversionHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> PackTypeExistsAsync(int packTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.PackType
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = packTypeId });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int miscMasterId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = miscMasterId });
            return count > 0;
        }

        public async Task<bool> LotMasterExistsAsync(int lotId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.LotMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = lotId });
            return count > 0;
        }

        private async Task PopulateCrossModuleNamesAsync(List<YarnConversionHeaderDto> list)
        {
            var unitIds = list.Select(x => x.UnitId).Distinct();
            var units = await _unitLookup.GetByIdsAsync(unitIds);
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var allItemIds = list.SelectMany(x => new[] { x.OldItemId, x.ItemId }).Distinct();
            var items = await _itemLookup.GetByIdsAsync(allItemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var allWarehouseIds = list.SelectMany(x => new[] { x.OldWarehouseId, x.WarehouseId }).Distinct();
            var warehouses = await _warehouseLookup.GetByIdsAsync(allWarehouseIds);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

            var allBinIds = list.SelectMany(x => new[] { x.OldBinId, x.BinId }).Distinct();
            var bins = await _binLookup.GetByIdsAsync(allBinIds);
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            foreach (var row in list)
            {
                row.UnitName         = unitDict.TryGetValue(row.UnitId,       out var un)  ? un  : null;
                row.OldItemName      = itemDict.TryGetValue(row.OldItemId,     out var oin) ? oin : null;
                row.ItemName         = itemDict.TryGetValue(row.ItemId,        out var inn) ? inn : null;
                row.OldWarehouseName = warehouseDict.TryGetValue(row.OldWarehouseId, out var owh) ? owh : null;
                row.WarehouseName    = warehouseDict.TryGetValue(row.WarehouseId,    out var wh)  ? wh  : null;
                row.OldBinName       = binDict.TryGetValue(row.OldBinId,  out var ob) ? ob : null;
                row.BinName          = binDict.TryGetValue(row.BinId,     out var bn) ? bn : null;
            }
        }
    }
}
