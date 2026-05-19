using System.Data;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemCategory
{
    public class ItemCategoryQueryRepository : IItemCategoryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPurchaseItemCategoryValidation _purchaseItemCategoryValidation;
        private readonly IModuleLookup _moduleLookup;
        private readonly IUnitLookup _unitLookup;

        public ItemCategoryQueryRepository(
            IDbConnection dbConnection,
            IPurchaseItemCategoryValidation purchaseItemCategoryValidation,
            IModuleLookup moduleLookup,
            IUnitLookup unitLookup)
        {
            _dbConnection = dbConnection;
            _purchaseItemCategoryValidation = purchaseItemCategoryValidation;
            _moduleLookup = moduleLookup;
            _unitLookup = unitLookup;
        }

        public async Task<ItemCategoryDto?> GetByIdAsync(int Id)
        {
            const string sql = @"
                SELECT
                    IC.Id,
                    IC.ItemCategoryName,
                    IG.Id AS ItemGroupId,
                    IG.ItemGroupName,
                    IC.IsGroup,
                    CASE WHEN IC.ParentCategoryId = IC.Id THEN NULL ELSE IC.ParentCategoryId END AS ParentCategoryId,
                    CASE WHEN IC.ParentCategoryId = IC.Id THEN NULL ELSE IC1.ItemCategoryName END AS ParentCategoryName,
                    IC.IsBudgetApplicable,
                    IC.EmergencyPOById,
                    EPOB.Description AS EmergencyPOByName,
                    IC.EmergencyValueLimit,
                    IC.EmergencyActionId,
                    EA.Description AS EmergencyActionName,
                    IC.IsActive,
                    IC.IsDeleted,
                    IC.CreatedBy, IC.CreatedDate, IC.CreatedByName, IC.CreatedIP,
                    IC.ModifiedBy, IC.ModifiedDate, IC.ModifiedByName, IC.ModifiedIP
                FROM Inventory.ItemCategory IC
                JOIN Inventory.ItemGroup IG ON IG.Id = IC.ItemGroupId
                LEFT JOIN Inventory.ItemCategory IC1 ON IC.ParentCategoryId = IC1.Id
                LEFT JOIN Inventory.MiscMaster EPOB ON EPOB.Id = IC.EmergencyPOById AND EPOB.IsDeleted = 0
                LEFT JOIN Inventory.MiscMaster EA ON EA.Id = IC.EmergencyActionId AND EA.IsDeleted = 0
                WHERE IC.IsDeleted = 0
                AND IC.Id = @Id;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ItemCategoryDto>(sql, new { Id = Id });

            if (dto != null)
            {
                const string joinSql = @"SELECT ModuleId FROM Inventory.ItemCategoryModule WHERE ItemCategoryId = @Id;";
                var joinedModuleIds = (await _dbConnection.QueryAsync<int>(joinSql, new { Id = Id })).ToList();

                if (joinedModuleIds.Count > 0)
                {
                    var modules = await _moduleLookup.GetAllModuleAsync();
                    var moduleDict = modules.ToDictionary(m => m.ModuleId, m => m.ModuleName);
                    dto.Modules = joinedModuleIds
                        .Select(mid => new ModuleLookupDto
                        {
                            ModuleId = mid,
                            ModuleName = moduleDict.TryGetValue(mid, out var name) ? name : null
                        })
                        .ToList();
                }

                dto.SampleQuantities = await GetSampleQuantitiesAsync(Id);
            }

            return dto;
        }

        public async Task<(IEnumerable<dynamic>, int)> GetAllItemCategoryAsync(int PageNumber, int PageSize, string? SearchTerm, int? moduleId)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Inventory.ItemCategory IC
                WHERE IC.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (IC.ItemCategoryName LIKE @Search )")}}
                AND (@ModuleId IS NULL OR EXISTS (
                    SELECT 1 FROM Inventory.ItemCategoryModule ICM
                    WHERE ICM.ItemCategoryId = IC.Id AND ICM.ModuleId = @ModuleId));

                SELECT
                    IC.Id,
                    IC.ItemCategoryName,
                    IG.Id AS ItemGroupId,
                    IG.ItemGroupName,
                    IC.IsGroup,
                    CASE WHEN IC.ParentCategoryId = IC.Id THEN NULL ELSE IC.ParentCategoryId END AS ParentCategoryId,
                    CASE WHEN IC.ParentCategoryId = IC.Id THEN NULL ELSE IC1.ItemCategoryName END AS ParentCategoryName,
                    IC.IsBudgetApplicable,
                    IC.EmergencyPOById,
                    EPOB.Description AS EmergencyPOByName,
                    IC.EmergencyValueLimit,
                    IC.EmergencyActionId,
                    EA.Description AS EmergencyActionName,
                    IC.IsActive,
                    IC.IsDeleted,
                    IC.CreatedBy, IC.CreatedDate, IC.CreatedByName, IC.CreatedIP,
                    IC.ModifiedBy, IC.ModifiedDate, IC.ModifiedByName, IC.ModifiedIP
                FROM Inventory.ItemCategory IC
                JOIN Inventory.ItemGroup IG ON IG.Id = IC.ItemGroupId
                LEFT JOIN Inventory.ItemCategory IC1 ON IC.ParentCategoryId = IC1.Id
                LEFT JOIN Inventory.MiscMaster EPOB ON EPOB.Id = IC.EmergencyPOById AND EPOB.IsDeleted = 0
                LEFT JOIN Inventory.MiscMaster EA ON EA.Id = IC.EmergencyActionId AND EA.IsDeleted = 0
                WHERE IC.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (IC.ItemCategoryName LIKE @Search )")}}
                AND (@ModuleId IS NULL OR EXISTS (
                    SELECT 1 FROM Inventory.ItemCategoryModule ICM
                    WHERE ICM.ItemCategoryId = IC.Id AND ICM.ModuleId = @ModuleId))
                ORDER BY IC.Id DESC;

                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize = PageSize,
                ModuleId = moduleId
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var flatList = (await result.ReadAsync<ItemCategoryDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            if (flatList.Count > 0)
            {
                var categoryIds = flatList.Select(x => x.Id).ToList();
                const string joinSql = @"SELECT ItemCategoryId, ModuleId FROM Inventory.ItemCategoryModule WHERE ItemCategoryId IN @Ids;";
                var joinRows = (await _dbConnection.QueryAsync<(int ItemCategoryId, int ModuleId)>(joinSql, new { Ids = categoryIds })).ToList();

                var modules = await _moduleLookup.GetAllModuleAsync();
                var moduleDict = modules.ToDictionary(m => m.ModuleId, m => m.ModuleName);

                var modulesByCategory = joinRows
                    .GroupBy(j => j.ItemCategoryId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(j => new ModuleLookupDto
                        {
                            ModuleId = j.ModuleId,
                            ModuleName = moduleDict.TryGetValue(j.ModuleId, out var name) ? name : null
                        }).ToList());

                foreach (var node in flatList)
                {
                    node.Modules = modulesByCategory.TryGetValue(node.Id, out var list) ? list : new List<ModuleLookupDto>();
                }

                var sqByCategory = await GetSampleQuantitiesByCategoryIdsAsync(categoryIds);
                foreach (var node in flatList)
                {
                    node.SampleQuantities = sqByCategory.TryGetValue(node.Id, out var sq) ? sq : new List<SampleQuantityDto>();
                }
            }

            var ids = flatList.Select(x => x.Id).ToHashSet();
            foreach (var node in flatList)
            {
                if (!node.ParentCategoryId.HasValue
                    || node.ParentCategoryId == node.Id
                    || !ids.Contains(node.ParentCategoryId.Value))
                {
                    node.ParentCategoryId = null;
                }
            }

            var byId = flatList.ToDictionary(x => x.Id);
            foreach (var node in flatList)
            {
                if (node.ParentCategoryId is int pid &&
                    byId.TryGetValue(pid, out var parent) &&
                    parent.Id != node.Id)
                {
                    parent.SubGroups.Add(node);
                }
            }

            var roots = flatList
                .Where(x => x.ParentCategoryId == null)
                .OrderByDescending(x => x.Id);

            var pagedRoots = roots
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return (pagedRoots, totalCount);
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[ItemCategory] WHERE ParentCategoryId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemCategoryId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayRule] WHERE ItemCategoryId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { Id });
            if (result == 1) return true;

            if (await _purchaseItemCategoryValidation.HasLinkedItemCategoryAsync(Id)) return true;

            return false;
        }

        public async Task<List<InventoryManagement.Domain.Entities.Item.ItemCategory>> GetCategoryByIdsAsync(IEnumerable<int> ids)
        {
            var list = ids?.Distinct().ToList() ?? new();
            if (list.Count == 0) return new();

            const string sql = @"
                 SELECT
                    IC.Id,
                    IC.ItemCategoryName
                FROM Inventory.ItemCategory IC
                WHERE IC.IsDeleted = 0
                AND IC.Id IN @Ids;";

            var rows = await _dbConnection.QueryAsync<InventoryManagement.Domain.Entities.Item.ItemCategory>(sql, new { Ids = list });
            return rows.ToList();
        }

        public async Task<bool> IsLinkedWithActiveItemsAsync(int itemCategoryId)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[ItemCategory] WHERE ParentCategoryId = @ItemCategoryId AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemCategoryId = @ItemCategoryId AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayRule] WHERE ItemCategoryId = @ItemCategoryId AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { ItemCategoryId = itemCategoryId });
            if (result == 1) return true;

            if (await _purchaseItemCategoryValidation.HasActiveItemCategoryAsync(itemCategoryId)) return true;

            return false;
        }

        public async Task<List<ItemCategoryAutoCompleteDto>> GetItemCategoryAutoCompleteAsync(
            int? groupId,
            string searchPattern,
            bool isParent, int excludeId, int? moduleId)
        {
            searchPattern = searchPattern ?? string.Empty;

            const string sql = @"
                SELECT
                    IC.Id,
                    IC.ItemCategoryName,
                    IC1.ItemCategoryName AS ParentCategoryName,
                    IG.Id  AS ItemGroupId,
                    IG.ItemGroupName,
                    IC.EmergencyPOById,
                    EPOB.Description AS EmergencyPOByName,
                    IC.EmergencyValueLimit,
                    IC.EmergencyActionId,
                    EA.Description AS EmergencyActionName
                FROM Inventory.ItemCategory IC
                LEFT JOIN Inventory.ItemCategory IC1 ON IC.ParentCategoryId = IC1.Id
                LEFT JOIN Inventory.ItemGroup IG     ON IG.Id = IC.ItemGroupId
                LEFT JOIN Inventory.MiscMaster EPOB ON EPOB.Id = IC.EmergencyPOById AND EPOB.IsDeleted = 0
                LEFT JOIN Inventory.MiscMaster EA ON EA.Id = IC.EmergencyActionId AND EA.IsDeleted = 0
                WHERE IC.IsDeleted = 0
                AND IC.IsActive  = 1
                AND IC.ItemCategoryName LIKE @SearchPattern
                AND (@GroupId IS NULL OR IG.Id = @GroupId)
                AND IC.IsGroup = @IsGroup
                AND (@ExcludeId = 0 OR IC.Id <> @ExcludeId)
                AND (@ModuleId IS NULL OR EXISTS (
                    SELECT 1 FROM Inventory.ItemCategoryModule ICM
                    WHERE ICM.ItemCategoryId = IC.Id AND ICM.ModuleId = @ModuleId))
                ORDER BY IC.ItemCategoryName;";

            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%",
                GroupId = groupId,
                IsGroup = isParent ? 1 : 0,
                ExcludeId = excludeId,
                ModuleId = moduleId
            };

            var rows = (await _dbConnection.QueryAsync<ItemCategoryAutoCompleteDto>(sql, parameters)).ToList();

            if (rows.Count > 0)
            {
                var categoryIds = rows.Select(r => r.Id).ToList();
                const string joinSql = @"SELECT ItemCategoryId, ModuleId FROM Inventory.ItemCategoryModule WHERE ItemCategoryId IN @Ids;";
                var joinRows = (await _dbConnection.QueryAsync<(int ItemCategoryId, int ModuleId)>(joinSql, new { Ids = categoryIds })).ToList();

                var modules = await _moduleLookup.GetAllModuleAsync();
                var moduleDict = modules.ToDictionary(m => m.ModuleId, m => m.ModuleName);

                var modulesByCategory = joinRows
                    .GroupBy(j => j.ItemCategoryId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(j => new ModuleLookupDto
                        {
                            ModuleId = j.ModuleId,
                            ModuleName = moduleDict.TryGetValue(j.ModuleId, out var name) ? name : null
                        }).ToList());

                foreach (var row in rows)
                {
                    row.Modules = modulesByCategory.TryGetValue(row.Id, out var list) ? list : new List<ModuleLookupDto>();
                }
            }

            return rows;
        }

        public async Task<bool> NotFoundAsync(int Id)
        {
            var query = "SELECT COUNT(1) FROM Inventory.ItemCategory WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = Id });
            return count == 0;
        }

        public async Task<List<SampleQuantityDto>> GetSampleQuantitiesAsync(int itemCategoryId)
        {
            const string sql = @"
                SELECT
                    SQ.Id,
                    SQ.UnitId,
                    SQ.UOMId,
                    U.UOMName,
                    SQ.MaxSampleQuantity,
                    SQ.IsActive
                FROM Inventory.ItemCategoryUnitConfig SQ
                LEFT JOIN Inventory.UOM U ON U.Id = SQ.UOMId AND U.IsDeleted = 0
                WHERE SQ.ItemCategoryId = @Id AND SQ.IsDeleted = 0
                ORDER BY SQ.Id;";

            var rows = (await _dbConnection.QueryAsync<SampleQuantityDto>(sql, new { Id = itemCategoryId })).ToList();

            if (rows.Count == 0) return rows;

            var unitIds = rows.Select(r => r.UnitId).Distinct().ToList();
            var units = await _unitLookup.GetByIdsAsync(unitIds);
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            foreach (var r in rows)
            {
                r.UnitName = unitDict.TryGetValue(r.UnitId, out var name) ? name : null;
            }

            return rows;
        }

        public async Task<Dictionary<int, List<SampleQuantityDto>>> GetSampleQuantitiesByCategoryIdsAsync(IEnumerable<int> itemCategoryIds)
        {
            var list = itemCategoryIds?.Distinct().ToList() ?? new();
            if (list.Count == 0) return new Dictionary<int, List<SampleQuantityDto>>();

            const string sql = @"
                SELECT
                    SQ.Id,
                    SQ.ItemCategoryId,
                    SQ.UnitId,
                    SQ.UOMId,
                    U.UOMName,
                    SQ.MaxSampleQuantity,
                    SQ.IsActive
                FROM Inventory.ItemCategoryUnitConfig SQ
                LEFT JOIN Inventory.UOM U ON U.Id = SQ.UOMId AND U.IsDeleted = 0
                WHERE SQ.ItemCategoryId IN @Ids AND SQ.IsDeleted = 0
                ORDER BY SQ.ItemCategoryId, SQ.Id;";

            var rows = (await _dbConnection.QueryAsync<(int Id, int ItemCategoryId, int UnitId, int UOMId, string? UOMName, decimal MaxSampleQuantity, int IsActive)>(sql, new { Ids = list })).ToList();

            if (rows.Count == 0) return new Dictionary<int, List<SampleQuantityDto>>();

            var unitIds = rows.Select(r => r.UnitId).Distinct().ToList();
            var units = await _unitLookup.GetByIdsAsync(unitIds);
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            return rows
                .GroupBy(r => r.ItemCategoryId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new SampleQuantityDto
                    {
                        Id = r.Id,
                        UnitId = r.UnitId,
                        UnitName = unitDict.TryGetValue(r.UnitId, out var name) ? name : null,
                        UOMId = r.UOMId,
                        UOMName = r.UOMName,
                        MaxSampleQuantity = r.MaxSampleQuantity,
                        IsActive = r.IsActive
                    }).ToList());
        }

        public async Task<SampleQuantityDto?> GetSampleQuantityAsync(int itemCategoryId, int unitId, int uomId)
        {
            const string sql = @"
                SELECT TOP 1
                    SQ.Id,
                    SQ.UnitId,
                    SQ.UOMId,
                    U.UOMName,
                    SQ.MaxSampleQuantity,
                    SQ.IsActive
                FROM Inventory.ItemCategoryUnitConfig SQ
                LEFT JOIN Inventory.UOM U ON U.Id = SQ.UOMId AND U.IsDeleted = 0
                WHERE SQ.ItemCategoryId = @ItemCategoryId
                  AND SQ.UnitId          = @UnitId
                  AND SQ.UOMId           = @UOMId
                  AND SQ.IsDeleted       = 0
                  AND SQ.IsActive        = 1;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SampleQuantityDto>(sql, new { ItemCategoryId = itemCategoryId, UnitId = unitId, UOMId = uomId });

            if (dto != null)
            {
                var unit = await _unitLookup.GetByIdAsync(dto.UnitId);
                dto.UnitName = unit?.UnitName;
            }

            return dto;
        }

        public async Task<bool> UnitExistsForCategoryAsync(int itemCategoryId, int unitId, int uomId, int? excludeRowId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Inventory.ItemCategoryUnitConfig
                    WHERE ItemCategoryId = @ItemCategoryId
                      AND UnitId          = @UnitId
                      AND UOMId           = @UOMId
                      AND IsDeleted       = 0
                      AND (@ExcludeRowId IS NULL OR Id <> @ExcludeRowId)
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                ItemCategoryId = itemCategoryId,
                UnitId = unitId,
                UOMId = uomId,
                ExcludeRowId = excludeRowId
            });
        }

        public async Task<bool> IsLeafCategoryAsync(int itemCategoryId)
        {
            const string sql = @"
                SELECT IsGroup FROM Inventory.ItemCategory
                WHERE Id = @Id AND IsDeleted = 0;";

            var isGroup = await _dbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { Id = itemCategoryId });
            return isGroup.HasValue && isGroup.Value == 0;
        }
    }
}
