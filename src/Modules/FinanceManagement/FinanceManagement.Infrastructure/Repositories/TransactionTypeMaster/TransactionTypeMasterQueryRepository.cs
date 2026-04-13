using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster
{
    public class TransactionTypeMasterQueryRepository : ITransactionTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IModuleLookup _moduleLookup;
        private readonly IMenuLookup _menuLookup;

        public TransactionTypeMasterQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IModuleLookup moduleLookup,
            IMenuLookup menuLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _moduleLookup = moduleLookup;
            _menuLookup = menuLookup;
        }

        public async Task<(List<TransactionTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            var modules = await _moduleLookup.GetAllModuleAsync();
            var moduleDict = modules.ToDictionary(m => m.ModuleId, m => m.ModuleName);

            var menus = await _menuLookup.GetAllMenuAsync();
            var menuDict = menus.ToDictionary(m => m.MenuId, m => m.MenuName);

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM [Finance].[TransactionTypeMaster]
                WHERE IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (TypeName LIKE @Search OR ShortName LIKE @Search)")}};

                SELECT Id, UnitId, ModuleId, MenuId, TypeName, ShortName, Description,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[TransactionTypeMaster]
                WHERE IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (TypeName LIKE @Search OR ShortName LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<TransactionTypeMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            foreach (var item in list)
            {
                item.UnitName   = unitDict.TryGetValue(item.UnitId,    out var uName)  ? uName  : null;
                item.ModuleName = moduleDict.TryGetValue(item.ModuleId, out var mName)  ? mName  : null;
                item.MenuName   = menuDict.TryGetValue(item.MenuId,     out var mnName) ? mnName : null;
            }

            return (list, totalCount);
        }

        public async Task<TransactionTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, UnitId, ModuleId, MenuId, TypeName, ShortName, Description,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[TransactionTypeMaster]
                WHERE Id = @Id AND IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<TransactionTypeMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var units = await _unitLookup.GetAllUnitAsync();
                dto.UnitName = units.FirstOrDefault(u => u.UnitId == dto.UnitId)?.UnitName;

                var modules = await _moduleLookup.GetAllModuleAsync();
                dto.ModuleName = modules.FirstOrDefault(m => m.ModuleId == dto.ModuleId)?.ModuleName;

                var menus = await _menuLookup.GetAllMenuAsync();
                dto.MenuName = menus.FirstOrDefault(m => m.MenuId == dto.MenuId)?.MenuName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<TransactionTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, TypeName, ShortName
                FROM [Finance].[TransactionTypeMaster]
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (TypeName LIKE @Term OR ShortName LIKE @Term)
                ORDER BY TypeName ASC";

            var result = await _dbConnection.QueryAsync<TransactionTypeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> TypeNameExistsAsync(string typeName, int unitId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM [Finance].[TransactionTypeMaster]
                WHERE TypeName = @TypeName AND UnitId = @UnitId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeName = typeName.Trim(), UnitId = unitId, Id = id });
            return count > 0;
        }

        public async Task<bool> ShortNameExistsAsync(string shortName, int unitId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM [Finance].[TransactionTypeMaster]
                WHERE ShortName = @ShortName AND UnitId = @UnitId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ShortName = shortName.Trim(), UnitId = unitId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM [Finance].[TransactionTypeMaster]
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var units = await _unitLookup.GetAllUnitAsync();
            return units.Any(u => u.UnitId == unitId);
        }

        public async Task<bool> ModuleExistsAsync(int moduleId)
        {
            var modules = await _moduleLookup.GetAllModuleAsync();
            return modules.Any(m => m.ModuleId == moduleId);
        }

        public async Task<bool> MenuExistsAsync(int menuId)
        {
            var menus = await _menuLookup.GetAllMenuAsync();
            return menus.Any(m => m.MenuId == menuId);
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Finance].[DocumentSequence]
                    WHERE TransactionTypeId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsTransactionTypeMasterLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Finance].[DocumentSequence]
                    WHERE TransactionTypeId = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
