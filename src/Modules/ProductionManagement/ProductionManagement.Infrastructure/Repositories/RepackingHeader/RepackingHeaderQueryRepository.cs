using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;

namespace ProductionManagement.Infrastructure.Repositories.RepackingHeader
{
    public class RepackingHeaderQueryRepository : IRepackingHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly ISalesStockLedgerService _salesStockLedgerService;

        public RepackingHeaderQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            ISalesStockLedgerService salesStockLedgerService)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _salesStockLedgerService = salesStockLedgerService;
        }

        public async Task<(List<RepackingHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm, int? typeId = null)
        {
            const string countSql = @"
                SELECT COUNT(*)
                FROM Production.RepackingHeader h
                WHERE h.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR h.RepackDocNo LIKE '%' + @SearchTerm + '%')
                  AND (@TypeId IS NULL OR h.TypeId = @TypeId)";

            const string dataSql = @"
                SELECT
                    h.Id, h.UnitId, h.ProductionYear, h.RepackDocNo, h.RepackDate,
                    h.ItemId, h.PackTypeId, h.StartPackNo, h.EndPackNo,
                    h.NetWeightPerPack, h.TotalBags, h.NetWeight,
                    h.WarehouseId, h.BinId,
                    h.OldItemId, h.OldPackTypeId,
                    h.LooseConeKgs, h.LooseHandlingId,
                    h.FaultId, h.WasteTypeId, h.WasteQuantity, h.WasteReason,
                    h.Remarks, h.LotId, h.TypeId,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName,
                    pt.PackTypeName AS PackTypeName,
                    opt.PackTypeName AS OldPackTypeName,
                    lm.LotCode AS LotName,
                    lh.Description AS LooseHandlingName,
                    f.Description AS FaultName,
                    wt.Description AS WasteTypeName,
                    tp.Description AS TypeName
                FROM Production.RepackingHeader h
                LEFT JOIN Production.PackType pt ON h.PackTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Production.PackType opt ON h.OldPackTypeId = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.LotMaster lm ON h.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.MiscMaster lh ON h.LooseHandlingId = lh.Id AND lh.IsDeleted = 0
                LEFT JOIN Production.MiscMaster f ON h.FaultId = f.Id AND f.IsDeleted = 0
                LEFT JOIN Production.MiscMaster wt ON h.WasteTypeId = wt.Id AND wt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster tp ON h.TypeId = tp.Id AND tp.IsDeleted = 0
                WHERE h.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR h.RepackDocNo LIKE '%' + @SearchTerm + '%')
                  AND (@TypeId IS NULL OR h.TypeId = @TypeId)
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (pageNumber - 1) * pageSize;
            var parameters = new { SearchTerm = searchTerm, TypeId = typeId, Offset = offset, PageSize = pageSize };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var headers = (await _dbConnection.QueryAsync<RepackingHeaderDto>(dataSql, parameters)).ToList();

            if (headers.Count > 0)
            {
                // Set computed IsRepacking
                foreach (var h in headers)
                    h.IsRepacking = h.ItemId == h.OldItemId;

                await PopulateCrossModuleNamesAsync(headers);
            }

            return (headers, totalCount);
        }

        public async Task<RepackingHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id, h.UnitId, h.ProductionYear, h.RepackDocNo, h.RepackDate,
                    h.ItemId, h.PackTypeId, h.StartPackNo, h.EndPackNo,
                    h.NetWeightPerPack, h.TotalBags, h.NetWeight,
                    h.WarehouseId, h.BinId,
                    h.OldItemId, h.OldPackTypeId,
                    h.LooseConeKgs, h.LooseHandlingId,
                    h.FaultId, h.WasteTypeId, h.WasteQuantity, h.WasteReason,
                    h.Remarks, h.LotId, h.TypeId,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName,
                    pt.PackTypeName AS PackTypeName,
                    opt.PackTypeName AS OldPackTypeName,
                    lm.LotCode AS LotName,
                    lh.Description AS LooseHandlingName,
                    f.Description AS FaultName,
                    wt.Description AS WasteTypeName,
                    tp.Description AS TypeName
                FROM Production.RepackingHeader h
                LEFT JOIN Production.PackType pt ON h.PackTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Production.PackType opt ON h.OldPackTypeId = opt.Id AND opt.IsDeleted = 0
                LEFT JOIN Production.LotMaster lm ON h.LotId = lm.Id AND lm.IsDeleted = 0
                LEFT JOIN Production.MiscMaster lh ON h.LooseHandlingId = lh.Id AND lh.IsDeleted = 0
                LEFT JOIN Production.MiscMaster f ON h.FaultId = f.Id AND f.IsDeleted = 0
                LEFT JOIN Production.MiscMaster wt ON h.WasteTypeId = wt.Id AND wt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster tp ON h.TypeId = tp.Id AND tp.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            const string detailSql = @"
                SELECT
                    d.Id, d.RepackHeaderId,
                    d.StartPackNo, d.EndPackNo,
                    d.OldStartPackNo, d.OldEndPackNo
                FROM Production.RepackingDetail d
                WHERE d.RepackHeaderId = @Id
                ORDER BY d.Id";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<RepackingHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            header.IsRepacking = header.ItemId == header.OldItemId;

            // Populate cross-module names for header
            await PopulateCrossModuleNamesAsync(new List<RepackingHeaderDto> { header });

            // Fetch details
            var details = (await _dbConnection.QueryAsync<RepackingDetailDto>(detailSql, new { Id = id })).ToList();

            // Populate detail fields from Sales.StockLedger
            if (details.Count > 0)
            {
                var warehouseIds = new HashSet<int>();
                var binIds = new HashSet<int>();

                foreach (var detail in details)
                {
                    var sourceInfo = await _salesStockLedgerService.GetPackSourceInfoAsync(
                        detail.OldStartPackNo, detail.OldEndPackNo,
                        header.ProductionYear, header.UnitId);

                    if (sourceInfo != null)
                    {
                        detail.OldNetWeightPerPack = sourceInfo.OldNetWeightPerPack;
                        detail.OldTotalBags = sourceInfo.OldTotalBags;
                        detail.OldNetWeight = sourceInfo.OldNetWeight;
                        detail.OldLotId = sourceInfo.LotId;
                        detail.OldWarehouseId = sourceInfo.WarehouseId;
                        detail.OldBinId = sourceInfo.BinId;

                        if (sourceInfo.WarehouseId > 0) warehouseIds.Add(sourceInfo.WarehouseId);
                        if (sourceInfo.BinId > 0) binIds.Add(sourceInfo.BinId);
                    }
                }

                if (warehouseIds.Count > 0)
                {
                    var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                    var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                    var bins = await _binLookup.GetByIdsAsync(binIds);
                    var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

                    foreach (var detail in details)
                    {
                        detail.OldWarehouseName = warehouseDict.TryGetValue(detail.OldWarehouseId, out var wn) ? wn : null;
                        detail.OldBinName = binDict.TryGetValue(detail.OldBinId, out var bn) ? bn : null;
                    }
                }
            }

            header.Details = details;
            return header;
        }

        public async Task<IReadOnlyList<RepackingHeaderLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct, int? typeId = null)
        {
            const string sql = @"
                SELECT Id, RepackDocNo, RepackDate
                FROM Production.RepackingHeader
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (@Term = '' OR RepackDocNo LIKE '%' + @Term + '%')
                  AND (@TypeId IS NULL OR TypeId = @TypeId)
                ORDER BY RepackDocNo ASC";

            var result = await _dbConnection.QueryAsync<RepackingHeaderLookupDto>(sql, new { Term = term, TypeId = typeId });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.RepackingHeader WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> PackTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.PackType WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> LotMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Production.LotMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        private async Task PopulateCrossModuleNamesAsync(List<RepackingHeaderDto> headers)
        {
            // Collect all cross-module IDs
            var unitIds = headers.Select(h => h.UnitId).Distinct();
            var itemIds = headers.Select(h => h.ItemId)
                .Union(headers.Select(h => h.OldItemId))
                .Distinct();
            var warehouseIds = headers.Select(h => h.WarehouseId).Distinct();
            var binIds = headers.Select(h => h.BinId).Distinct();

            // Batch fetch
            var units = await _unitLookup.GetByIdsAsync(unitIds);
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

            var bins = await _binLookup.GetByIdsAsync(binIds);
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);

            // Populate
            foreach (var h in headers)
            {
                h.UnitName = unitDict.TryGetValue(h.UnitId, out var un) ? un : null;
                h.ItemName = itemDict.TryGetValue(h.ItemId, out var inm) ? inm : null;
                h.OldItemName = itemDict.TryGetValue(h.OldItemId, out var oinm) ? oinm : null;
                h.WarehouseName = warehouseDict.TryGetValue(h.WarehouseId, out var wn) ? wn : null;
                h.BinName = binDict.TryGetValue(h.BinId, out var bn) ? bn : null;
            }
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // RepackingDetail has NO IsDeleted column — any record means linked
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Production].[RepackingDetail]
                    WHERE RepackHeaderId = @Id
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsRepackingHeaderLinkedAsync(int id)
        {
            // RepackingDetail has NO IsActive column — same check as SoftDelete
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Production].[RepackingDetail]
                    WHERE RepackHeaderId = @Id
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
