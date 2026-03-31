using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.RepackingMaster
{
    public class RepackingMasterQueryRepository : IRepackingMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public RepackingMasterQueryRepository(
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

        public async Task<(List<RepackingMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.RepackingMaster rm
                WHERE rm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR rm.RepackDocNo LIKE @SearchTerm);

                SELECT
                    rm.Id, rm.UnitId, rm.ProductionYear, rm.RepackDocNo, rm.RepackDate,
                    rm.ItemId, 
                    rm.OldPackTypeId,
                    opt.PackTypeName AS OldPackTypeName,
                    rm.OldNetWeightPerPack, rm.OldStartPackNo, rm.OldEndPackNo,
                    rm.OldTotalBags, rm.OldNetWeight,
                    rm.OldWarehouseId, rm.OldBinId,
                    rm.PackTypeId,
                    npt.PackTypeName AS PackTypeName,
                    rm.NetWeightPerPack, rm.StartPackNo, rm.EndPackNo,
                    rm.TotalBags, rm.NetWeight,
                    rm.WarehouseId, rm.BinId,
                    rm.LooseConeKgs, rm.LooseHandlingId,
                    lh.Description AS LooseHandlingName,
                    rm.Remarks,
                    rm.IsActive, rm.IsDeleted,
                    rm.CreatedBy, rm.CreatedDate, rm.CreatedByName,
                    rm.ModifiedBy, rm.ModifiedDate, rm.ModifiedByName
                FROM Production.RepackingMaster rm
                LEFT JOIN Production.PackType opt ON rm.OldPackTypeId = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.PackType npt ON rm.PackTypeId = npt.Id AND npt.IsDeleted = 0                
                LEFT JOIN Production.MiscMaster lh ON rm.LooseHandlingId = lh.Id AND lh.IsDeleted = 0
                WHERE rm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR rm.RepackDocNo LIKE @SearchTerm)
                ORDER BY rm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<RepackingMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                await PopulateCrossModuleNamesAsync(list);
            }

            return (list, totalCount);
        }

        public async Task<RepackingMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    rm.Id, rm.UnitId, rm.ProductionYear, rm.RepackDocNo, rm.RepackDate,
                    rm.ItemId, 
                    rm.OldPackTypeId,
                    opt.PackTypeName AS OldPackTypeName,
                    rm.OldNetWeightPerPack, rm.OldStartPackNo, rm.OldEndPackNo,
                    rm.OldTotalBags, rm.OldNetWeight,
                    rm.OldWarehouseId, rm.OldBinId,
                    rm.PackTypeId,
                    npt.PackTypeName AS PackTypeName,
                    rm.NetWeightPerPack, rm.StartPackNo, rm.EndPackNo,
                    rm.TotalBags, rm.NetWeight,
                    rm.WarehouseId, rm.BinId,
                    rm.LooseConeKgs, rm.LooseHandlingId,
                    lh.Description AS LooseHandlingName,
                    rm.Remarks,
                    rm.IsActive, rm.IsDeleted,
                    rm.CreatedBy, rm.CreatedDate, rm.CreatedByName,
                    rm.ModifiedBy, rm.ModifiedDate, rm.ModifiedByName
                FROM Production.RepackingMaster rm
                LEFT JOIN Production.PackType opt ON rm.OldPackTypeId = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.PackType npt ON rm.PackTypeId = npt.Id AND npt.IsDeleted = 0                
                LEFT JOIN Production.MiscMaster lh ON rm.LooseHandlingId = lh.Id AND lh.IsDeleted = 0
                WHERE rm.Id = @Id AND rm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<RepackingMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                await PopulateCrossModuleNamesAsync(new List<RepackingMasterDto> { dto });
            }

            return dto;
        }

        public async Task<IReadOnlyList<RepackingMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, RepackDocNo, RepackDate
                FROM Production.RepackingMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (RepackDocNo LIKE @Term)
                ORDER BY RepackDocNo ASC";

            var result = await _dbConnection.QueryAsync<RepackingMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string repackDocNo, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.RepackingMaster
                WHERE RepackDocNo = @RepackDocNo AND IsDeleted = 0
                  AND (@Id IS NULL OR Id != @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { RepackDocNo = repackDocNo, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.RepackingMaster
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

        private async Task PopulateCrossModuleNamesAsync(List<RepackingMasterDto> list)
        {
            // Item names (cross-module: InventoryManagement)
            var itemIds = list.Select(x => x.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            // Unit names (cross-module: UserManagement)
            var unitIds = list.Select(x => x.UnitId).Distinct();
            var units = await _unitLookup.GetByIdsAsync(unitIds);
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            // Warehouse names (cross-module: WarehouseManagement)
            var warehouseIds = list.SelectMany(x => new[] { x.OldWarehouseId, x.WarehouseId }).Distinct();
            var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

            // Bin names (cross-module: WarehouseManagement)
            var binIds = list.SelectMany(x => new[] { x.OldBinId, x.BinId }).Distinct();
            var bins = await _binLookup.GetByIdsAsync(binIds);
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            foreach (var item in list)
            {
                item.ItemName = itemDict.TryGetValue(item.ItemId, out var itemName) ? itemName : null;
                item.UnitName = unitDict.TryGetValue(item.UnitId, out var unitName) ? unitName : null;
                item.OldWarehouseName = warehouseDict.TryGetValue(item.OldWarehouseId, out var oldWhName) ? oldWhName : null;
                item.WarehouseName = warehouseDict.TryGetValue(item.WarehouseId, out var whName) ? whName : null;
                item.OldBinName = binDict.TryGetValue(item.OldBinId, out var oldBinName) ? oldBinName : null;
                item.BinName = binDict.TryGetValue(item.BinId, out var binName) ? binName : null;
            }
        }
    }
}
