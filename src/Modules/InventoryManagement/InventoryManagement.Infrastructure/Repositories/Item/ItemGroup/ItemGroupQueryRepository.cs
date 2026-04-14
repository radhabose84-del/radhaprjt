#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using Contracts.Interfaces;
using Contracts.Interfaces.Validations.WarehouseManagement;
using Dapper;

namespace  InventoryManagement.Infrastructure.Repositories.Item.ItemGroup
{
    public class ItemGroupQueryRepository : IItemGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IWarehouseItemGroupValidation _warehouseItemGroupValidation;
        private readonly IDataAccessFilter _dataAccessFilter;

        public ItemGroupQueryRepository(IDbConnection dbConnection,
            IWarehouseItemGroupValidation warehouseItemGroupValidation,
            IDataAccessFilter dataAccessFilter)
        {
            _dbConnection = dbConnection;
            _warehouseItemGroupValidation = warehouseItemGroupValidation;
            _dataAccessFilter = dataAccessFilter;
        }
        public async Task<ItemGroupDto> GetByIdAsync(int Id)
        {
            // Role-based item group filtering
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            if (!accessCtx.BypassDataAccess && accessCtx.AllowedItemGroupIds != null)
            {
                if (accessCtx.AllowedItemGroupIds.Count == 0 || !accessCtx.AllowedItemGroupIds.Contains(Id))
                    return null;
            }

            const string query = @" select
                    Id,ItemGroupCode, ItemGroupName,UnitId
                    ,IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP, ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                    FROM  Inventory.ItemGroup WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<ItemGroupDto>(query, new { Id });
        }
        public async Task<(IEnumerable<dynamic>, int)> GetAllItemGroupAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            // Role-based item group filtering
            // null = feature not configured (no filtering); empty = user has no access
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            bool applyRoleFilter = !accessCtx.BypassDataAccess && accessCtx.AllowedItemGroupIds != null;
            if (applyRoleFilter && accessCtx.AllowedItemGroupIds!.Count == 0)
            {
                // Mappings exist but none for this user — return empty
                return (new List<ItemGroupDto>(), 0);
            }

            var roleFilterClause = applyRoleFilter ? " AND Id IN @AllowedGroupIds " : string.Empty;

            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*)
            FROM Inventory.ItemGroup
            WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemGroupName LIKE @Search)")}}
            {{roleFilterClause}};

            SELECT
                Id,ItemGroupCode, ItemGroupName,UnitId
                ,IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP, ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM  Inventory.ItemGroup             WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemGroupName LIKE @Search )")}}
            {{roleFilterClause}}
            ORDER BY Id desc
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new DynamicParameters();
            parameters.Add("Search", $"%{SearchTerm}%");
            parameters.Add("Offset", (PageNumber - 1) * PageSize);
            parameters.Add("PageSize", PageSize);
            if (applyRoleFilter)
                parameters.Add("AllowedGroupIds", accessCtx.AllowedItemGroupIds!.ToArray());

            var notificationConfig = await _dbConnection.QueryMultipleAsync(query, parameters);
            var notificationConfigList = (await notificationConfig.ReadAsync<ItemGroupDto>()).ToList();
            int totalCount = (await notificationConfig.ReadFirstAsync<int>());
            return (notificationConfigList, totalCount);
        }
        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[ItemCategory] WHERE ItemGroupId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemGroupId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayRule] WHERE ItemGroupId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { Id });
            if (result == 1) return true;

            // Cross-module check
            if (await _warehouseItemGroupValidation.HasLinkedItemGroupAsync(Id)) return true;

            return false;
        }

        public async Task<bool> IsItemGroupLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[ItemCategory] WHERE ItemGroupId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemGroupId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayRule] WHERE ItemGroupId = @id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            if (result == 1) return true;

            // Cross-module check
            if (await _warehouseItemGroupValidation.HasActiveItemGroupAsync(id)) return true;

            return false;
        }
        public async Task<bool> NotFoundAsync(int Id)
        {
            var query = "SELECT COUNT(1) FROM Inventory.ItemGroup WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = Id });
            return count > 0;
        }
        public async Task<List<ItemGroupAutoCompleteDto>> GetItemGroupAutoCompleteAsync(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty;

            // Role-based item group filtering
            // null = feature not configured (no filtering); empty = user has no access
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            bool applyRoleFilter = !accessCtx.BypassDataAccess && accessCtx.AllowedItemGroupIds != null;
            if (applyRoleFilter && accessCtx.AllowedItemGroupIds!.Count == 0)
            {
                return new List<ItemGroupAutoCompleteDto>();
            }

            var query = @"
             SELECT IC.Id, IC.ItemGroupName
            FROM Inventory.ItemGroup IC
            WHERE IC.IsDeleted = 0 and IC.IsActive = 1
            AND ItemGroupName LIKE @SearchPattern";

            if (applyRoleFilter)
                query += " AND IC.Id IN @AllowedGroupIds";

            var parameters = new DynamicParameters();
            parameters.Add("SearchPattern", $"%{searchPattern}%");
            if (applyRoleFilter)
                parameters.Add("AllowedGroupIds", accessCtx.AllowedItemGroupIds!.ToArray());

            var notificationConfig = await _dbConnection.QueryAsync<ItemGroupAutoCompleteDto>(query, parameters);
            return notificationConfig.ToList();
        }
        
        public async Task<List<InventoryManagement.Domain.Entities.Item.ItemGroup>> GetAllItemGroupsAsync()
        {
            // Role-based item group filtering
            // null = feature not configured (no filtering); empty = user has no access
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            bool applyRoleFilter = !accessCtx.BypassDataAccess && accessCtx.AllowedItemGroupIds != null;
            if (applyRoleFilter && accessCtx.AllowedItemGroupIds!.Count == 0)
            {
                return new List<InventoryManagement.Domain.Entities.Item.ItemGroup>();
            }

            var sql = @"
                SELECT
                    Id,
                    UnitId,
                    ItemGroupCode,
                    ItemGroupName,
                    IsActive
                FROM [Inventory].[ItemGroup]
                WHERE IsDeleted = 0";

            if (applyRoleFilter)
                sql += " AND Id IN @AllowedGroupIds";

            sql += " ORDER BY ItemGroupName ASC;";

            var parameters = new DynamicParameters();
            if (applyRoleFilter)
                parameters.Add("AllowedGroupIds", accessCtx.AllowedItemGroupIds!.ToArray());

            var result = await _dbConnection.QueryAsync<InventoryManagement.Domain.Entities.Item.ItemGroup>(sql, parameters);
            return result.AsList();
        }

    }
}