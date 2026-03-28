using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.Infrastructure.Repositories.Repacking
{
    public class RepackingQueryRepository : IRepackingQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public RepackingQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup   = unitLookup;
            _itemLookup   = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup    = binLookup;
        }

        public async Task<(List<RepackingHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            const string sql = @"
                SELECT
                    h.Id, h.UnitId, h.ProductionYear, h.RepackingNo, h.RepackingDate,
                    h.TotalBags, h.NetWeight, h.LooseConeKgs,
                    h.OldPackHeaderId, p.PackNo AS OldPackNo,
                    h.LooseHandlingId, m.Description AS LooseHandlingName,
                    h.Remarks,
                    CAST(h.IsActive  AS BIT) AS IsActive,
                    CAST(h.IsDeleted AS BIT) AS IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Production.RepackingHeader h
                LEFT JOIN Production.ProductionPackHeader p
                    ON h.OldPackHeaderId = p.Id AND p.IsDeleted = 0
                LEFT JOIN Production.MiscMaster m
                    ON h.LooseHandlingId = m.Id AND m.IsDeleted = 0
                WHERE h.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR h.RepackingNo LIKE '%' + @SearchTerm + '%')
                ORDER BY h.Id DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Production.RepackingHeader
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL OR RepackingNo LIKE '%' + @SearchTerm + '%');";

            using var multi = await _dbConnection.QueryMultipleAsync(sql,
                new { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm });

            var items      = (await multi.ReadAsync<RepackingHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var item in items)
            {
                if (unitDict.TryGetValue(item.UnitId, out var unitName))
                    item.UnitName = unitName;
            }

            return (items, totalCount);
        }

        public async Task<RepackingHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id, h.UnitId, h.ProductionYear, h.RepackingNo, h.RepackingDate,
                    h.TotalBags, h.NetWeight, h.LooseConeKgs,
                    h.OldPackHeaderId, p.PackNo AS OldPackNo,
                    h.LooseHandlingId, m.Description AS LooseHandlingName,
                    h.Remarks,
                    CAST(h.IsActive  AS BIT) AS IsActive,
                    CAST(h.IsDeleted AS BIT) AS IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Production.RepackingHeader h
                LEFT JOIN Production.ProductionPackHeader p
                    ON h.OldPackHeaderId = p.Id AND p.IsDeleted = 0
                LEFT JOIN Production.MiscMaster m
                    ON h.LooseHandlingId = m.Id AND m.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            const string detailSql = @"
                SELECT
                    d.Id, d.RepackingHeaderId, d.ItemId, d.LotId, l.LotCode,
                    d.BinId, d.WarehouseId,
                    d.PackTypeId, pt.PackTypeName,
                    d.StartPackNo, d.EndPackNo,
                    d.NetWeightPerPack, d.TotalBags, d.NetWeight, d.OldPackDetailId
                FROM Production.RepackingDetail d
                LEFT JOIN Production.LotMaster l
                    ON d.LotId = l.Id AND l.IsDeleted = 0
                LEFT JOIN Production.PackType pt
                    ON d.PackTypeId = pt.Id AND pt.IsDeleted = 0
                WHERE d.RepackingHeaderId = @Id;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<RepackingHeaderDto>(
                headerSql, new { Id = id });

            if (header == null)
                return null;

            var details = (await _dbConnection.QueryAsync<RepackingDetailDto>(
                detailSql, new { Id = id })).ToList();

            var units      = await _unitLookup.GetAllUnitAsync();
            var itemIds    = details.Select(d => d.ItemId).Distinct();
            var items      = await _itemLookup.GetByIdsAsync(itemIds);
            var warehouses = await _warehouseLookup.GetAllAsync();
            var bins       = await _binLookup.GetAllAsync();

            var unitDict      = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            var itemDict      = items.ToDictionary(i => i.Id, i => i.ItemName);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);
            var binDict       = bins.ToDictionary(b => b.Id, b => b.BinName);

            if (unitDict.TryGetValue(header.UnitId, out var unitName))
                header.UnitName = unitName;

            foreach (var detail in details)
            {
                if (itemDict.TryGetValue(detail.ItemId,           out var itemName))      detail.ItemName      = itemName;
                if (warehouseDict.TryGetValue(detail.WarehouseId, out var warehouseName)) detail.WarehouseName = warehouseName;
                if (binDict.TryGetValue(detail.BinId,            out var binName))       detail.BinName       = binName;
            }

            header.RepackingDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<RepackingLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, RepackingNo
                FROM Production.RepackingHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term = '' OR RepackingNo LIKE @Term + '%')
                ORDER BY RepackingNo ASC;";

            var result = await _dbConnection.QueryAsync<RepackingLookupDto>(sql, new { Term = term });
            return result.ToList().AsReadOnly();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(1) = 0 THEN 1 ELSE 0 END
                FROM Production.RepackingHeader WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> OldPackHeaderExistsAsync(int oldPackHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
                FROM Production.ProductionPackHeader WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = oldPackHeaderId });
        }

        public async Task<bool> PackDetailExistsAsync(int oldPackDetailId)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
                FROM Production.ProductionPackDetail WHERE Id = @Id;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = oldPackDetailId });
        }

        public async Task<bool> LotExistsAsync(int lotId)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
                FROM Production.LotMaster WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = lotId });
        }

        public async Task<bool> PackTypeExistsAsync(int packTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
                FROM Production.PackType WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = packTypeId });
        }
    }
}
