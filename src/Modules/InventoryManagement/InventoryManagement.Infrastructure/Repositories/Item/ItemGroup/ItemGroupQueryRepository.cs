#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using Dapper;

namespace  InventoryManagement.Infrastructure.Repositories.Item.ItemGroup
{
    public class ItemGroupQueryRepository : IItemGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ItemGroupQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<ItemGroupDto> GetByIdAsync(int Id)
        {
            const string query = @" select 
                    Id,ItemGroupCode, ItemGroupName,UnitId 
                    ,IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP, ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                    FROM  Inventory.ItemGroup WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<ItemGroupDto>(query, new { Id });
        }
        public async Task<(IEnumerable<dynamic>, int)> GetAllItemGroupAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) 
            FROM Inventory.ItemGroup 
            WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemGroupName LIKE @Search)")}};

            SELECT 
                Id,ItemGroupCode, ItemGroupName,UnitId 
                ,IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP, ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM  Inventory.ItemGroup             WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ItemGroupName LIKE @Search )")}}
            ORDER BY Id desc
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

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
            return result == 1;
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
            return result == 1;
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
            const string query = @"
             SELECT IC.Id, IC.ItemGroupName
            FROM Inventory.ItemGroup IC            
            WHERE IC.IsDeleted = 0 and IC.IsActive = 1
            AND ItemGroupName LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"
            };
            var notificationConfig = await _dbConnection.QueryAsync<ItemGroupAutoCompleteDto>(query, parameters);
            return notificationConfig.ToList();
        }
        
        public async Task<List<InventoryManagement.Domain.Entities.Item.ItemGroup>> GetAllItemGroupsAsync()
        {
            const string sql = @"
                SELECT 
                    Id,
                    UnitId,
                    ItemGroupCode,
                    ItemGroupName,
                    IsActive
                FROM [Inventory].[ItemGroup]
                WHERE IsDeleted = 0
                ORDER BY ItemGroupName ASC;
            ";

            var result = await _dbConnection.QueryAsync<InventoryManagement.Domain.Entities.Item.ItemGroup>(sql);
            return result.AsList();
        }      
    }
}