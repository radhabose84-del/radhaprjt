using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Dto;

namespace InventoryManagement.Infrastructure.Repositories.UsageType
{
    public class UsageTypeQueryRepository : IUsageTypeQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IModuleLookup _moduleLookup;

        public UsageTypeQueryRepository(IDbConnection dbConnection, IModuleLookup moduleLookup)
        {
            _dbConnection = dbConnection;
            _moduleLookup = moduleLookup;
        }

        public async Task<(List<UsageTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT
                    Id, UsageTypeCode, UsageTypeName, Description, ModuleId,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Inventory.UsageType
                WHERE IsDeleted = 0";

            var countSql = @"
                SELECT COUNT(*)
                FROM Inventory.UsageType
                WHERE IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchFilter = " AND (UsageTypeCode LIKE @SearchTerm OR UsageTypeName LIKE @SearchTerm)";
                sql += searchFilter;
                countSql += searchFilter;
            }

            sql += " ORDER BY Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Offset = offset,
                PageSize = pageSize,
                SearchTerm = $"%{searchTerm}%"
            };

            var data = (await _dbConnection.QueryAsync<UsageTypeDto>(sql, parameters)).ToList();
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);

            // Populate ModuleName via cross-module lookup
            if (data.Count > 0)
            {
                var modules = await _moduleLookup.GetAllModuleAsync();
                var moduleDict = modules.ToDictionary(m => m.ModuleId, m => m.ModuleName);
                foreach (var dto in data)
                {
                    if (moduleDict.TryGetValue(dto.ModuleId, out var moduleName))
                        dto.ModuleName = moduleName;
                }
            }

            return (data, totalCount);
        }

        public async Task<UsageTypeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id, UsageTypeCode, UsageTypeName, Description, ModuleId,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Inventory.UsageType
                WHERE Id = @Id AND IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<UsageTypeDto>(sql, new { Id = id });

            if (dto != null)
            {
                var modules = await _moduleLookup.GetAllModuleAsync();
                var module = modules.FirstOrDefault(m => m.ModuleId == dto.ModuleId);
                dto.ModuleName = module?.ModuleName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<UsageTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, UsageTypeCode, UsageTypeName
                FROM Inventory.UsageType
                WHERE IsActive = 1 AND IsDeleted = 0
                    AND (UsageTypeCode LIKE @Term OR UsageTypeName LIKE @Term)
                ORDER BY UsageTypeName ASC";

            var result = await _dbConnection.QueryAsync<UsageTypeLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Inventory.UsageType WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> AlreadyExistsAsync(string usageTypeCode, int? id = null)
        {
            var sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Inventory.UsageType
                    WHERE UsageTypeCode = @UsageTypeCode AND IsDeleted = 0";

            if (id.HasValue)
                sql += " AND Id != @Id";

            sql += ") THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UsageTypeCode = usageTypeCode, Id = id });
        }

        public async Task<bool> ModuleExistsAsync(int moduleId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.Modules WHERE Id = @ModuleId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { ModuleId = moduleId });
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Inventory].[ItemUsageTypeMapping] WHERE UsageTypeId = @id
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

        public async Task<bool> IsUsageTypeLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Inventory].[ItemUsageTypeMapping] WHERE UsageTypeId = @id
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }
    }
}
