using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.LotMaster
{
    public class LotMasterQueryRepository : ILotMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IUnitLookup _unitLookup;

        public LotMasterQueryRepository(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IUnitLookup unitLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _unitLookup = unitLookup;
        }

        public async Task<(List<LotMasterDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (lm.LotCode LIKE @Search OR lm.BatchNumber LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Production.LotMaster lm
                LEFT JOIN Production.MiscMaster lt ON lm.LotTypeId = lt.Id AND lt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster st ON lm.StatusId  = st.Id AND st.IsDeleted = 0
                WHERE lm.IsDeleted = 0
                {searchFilter};

                SELECT
                    lm.Id, lm.LotCode, lm.BatchNumber,
                    lm.LotTypeId, lt.Description AS LotTypeName,
                    lm.ItemId,
                    lm.UnitId,
                    lm.StartDate,
                    lm.StatusId, st.Description AS StatusName,
                    lm.ProductionOrderRef,
                    lm.TotalProducedQty, lm.AvailableQty, lm.RunOutDate,
                    lm.Remarks, lm.IsActive, lm.IsDeleted,
                    lm.CreatedBy, lm.CreatedDate, lm.CreatedByName, lm.CreatedIP,
                    lm.ModifiedBy, lm.ModifiedDate, lm.ModifiedByName, lm.ModifiedIP
                FROM Production.LotMaster lm
                LEFT JOIN Production.MiscMaster lt ON lm.LotTypeId = lt.Id AND lt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster st ON lm.StatusId  = st.Id AND st.IsDeleted = 0
                WHERE lm.IsDeleted = 0
                {searchFilter}
                ORDER BY lm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<LotMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                var itemIds = list.Select(x => x.ItemId).Distinct();
                var unitIds = list.Select(x => x.UnitId).Distinct();

                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(x => x.Id);

                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(x => x.UnitId, x => x.UnitName);

                foreach (var dto in list)
                {
                    if (itemDict.TryGetValue(dto.ItemId, out var itemData))
                    {
                        dto.ItemCode = itemData.ItemCode;
                        dto.ItemName = itemData.ItemName;
                    }

                    if (unitDict.TryGetValue(dto.UnitId, out var unitName))
                        dto.UnitName = unitName;
                }
            }

            return (list, totalCount);
        }

        public async Task<LotMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    lm.Id, lm.LotCode, lm.BatchNumber,
                    lm.LotTypeId, lt.Description AS LotTypeName,
                    lm.ItemId,
                    lm.UnitId,
                    lm.StartDate,
                    lm.StatusId, st.Description AS StatusName,
                    lm.ProductionOrderRef,
                    lm.TotalProducedQty, lm.AvailableQty, lm.RunOutDate,
                    lm.Remarks, lm.IsActive, lm.IsDeleted,
                    lm.CreatedBy, lm.CreatedDate, lm.CreatedByName, lm.CreatedIP,
                    lm.ModifiedBy, lm.ModifiedDate, lm.ModifiedByName, lm.ModifiedIP
                FROM Production.LotMaster lm
                LEFT JOIN Production.MiscMaster lt ON lm.LotTypeId = lt.Id AND lt.IsDeleted = 0
                LEFT JOIN Production.MiscMaster st ON lm.StatusId  = st.Id AND st.IsDeleted = 0
                WHERE lm.Id = @Id AND lm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<LotMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId });
                var itemData = items.FirstOrDefault();
                if (itemData != null)
                {
                    dto.ItemCode = itemData.ItemCode;
                    dto.ItemName = itemData.ItemName;
                }

                var units = await _unitLookup.GetByIdsAsync(new[] { dto.UnitId });
                var unitData = units.FirstOrDefault();
                if (unitData != null)
                    dto.UnitName = unitData.UnitName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<LotMasterLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    lm.Id, lm.LotCode, lm.BatchNumber, lm.ItemId
                FROM Production.LotMaster lm
                WHERE lm.IsDeleted = 0 AND lm.IsActive = 1
                  AND (lm.LotCode LIKE @Term OR lm.BatchNumber LIKE @Term)
                ORDER BY lm.LotCode ASC";

            var result = (await _dbConnection.QueryAsync<LotMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct))).ToList();

            if (result.Count > 0)
            {
                var itemIds = result.Select(x => x.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
                var itemDict = items.ToDictionary(x => x.Id);

                foreach (var dto in result)
                {
                    if (itemDict.TryGetValue(dto.ItemId, out var itemData))
                        dto.ItemName = itemData.ItemName;
                }
            }

            return result;
        }

        public async Task<bool> AlreadyExistsAsync(string lotCode)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.LotMaster
                WHERE LotCode = @LotCode AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { LotCode = lotCode });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.LotMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> LotTypeExistsAsync(int lotTypeId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = lotTypeId });
            return count > 0;
        }

        public async Task<bool> StatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = statusId });
            return count > 0;
        }

        public async Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId }, ct);
            return items.Any();
        }

        public async Task<bool> UnitExistsAsync(int unitId, CancellationToken ct = default)
        {
            var units = await _unitLookup.GetByIdsAsync(new[] { unitId }, ct);
            return units.Any();
        }
    }
}
