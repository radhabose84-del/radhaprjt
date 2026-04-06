using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.FreightMaster
{
    public class FreightMasterQueryRepository : IFreightMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public FreightMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FreightMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE fm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause += @" AND (mode.Description LIKE @SearchTerm
                                   OR method.Description LIKE @SearchTerm)";
            }

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.FreightMaster fm
                LEFT JOIN Sales.MiscMaster mode ON fm.FreightModeId = mode.Id AND mode.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster method ON fm.RateMethodId = method.Id AND method.IsDeleted = 0
                {whereClause}";

            var dataSql = $@"
                SELECT fm.Id, fm.FreightModeId, fm.RateMethodId, fm.Rate,
                       fm.IsActive, fm.IsDeleted,
                       fm.CreatedBy, fm.CreatedDate, fm.CreatedByName,
                       fm.ModifiedBy, fm.ModifiedDate, fm.ModifiedByName,
                       mode.Description AS FreightModeName,
                       method.Description AS RateMethodName
                FROM Sales.FreightMaster fm
                LEFT JOIN Sales.MiscMaster mode ON fm.FreightModeId = mode.Id AND mode.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster method ON fm.RateMethodId = method.Id AND method.IsDeleted = 0
                {whereClause}
                ORDER BY fm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<FreightMasterDto>(dataSql, parameters)).ToList();

            return (data, totalCount);
        }

        public async Task<FreightMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT fm.Id, fm.FreightModeId, fm.RateMethodId, fm.Rate,
                       fm.IsActive, fm.IsDeleted,
                       fm.CreatedBy, fm.CreatedDate, fm.CreatedByName,
                       fm.ModifiedBy, fm.ModifiedDate, fm.ModifiedByName,
                       mode.Description AS FreightModeName,
                       method.Description AS RateMethodName
                FROM Sales.FreightMaster fm
                LEFT JOIN Sales.MiscMaster mode ON fm.FreightModeId = mode.Id AND mode.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster method ON fm.RateMethodId = method.Id AND method.IsDeleted = 0
                WHERE fm.Id = @Id AND fm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<FreightMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<FreightMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT fm.Id,
                       mode.Description AS FreightModeName,
                       method.Description AS RateMethodName,
                       fm.Rate
                FROM Sales.FreightMaster fm
                LEFT JOIN Sales.MiscMaster mode ON fm.FreightModeId = mode.Id AND mode.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster method ON fm.RateMethodId = method.Id AND method.IsDeleted = 0
                WHERE fm.IsActive = 1 AND fm.IsDeleted = 0
                  AND (mode.Description LIKE @Term OR method.Description LIKE @Term OR @Term = '')
                ORDER BY mode.Description, method.Description";

            var result = await _dbConnection.QueryAsync<FreightMasterLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> CompositeKeyExistsAsync(int freightModeId, int rateMethodId, int? id = null)
        {
            var sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.FreightMaster
                    WHERE FreightModeId = @FreightModeId
                      AND RateMethodId = @RateMethodId
                      AND IsDeleted = 0";

            if (id.HasValue)
                sql += " AND Id != @Id";

            sql += ") THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { FreightModeId = freightModeId, RateMethodId = rateMethodId, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Sales.FreightMaster
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.MiscMaster
                    WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsValidModeMethodCombinationAsync(int freightModeId, int rateMethodId)
        {
            const string sql = @"
                SELECT mode.Description AS ModeDescription,
                       method.Description AS MethodDescription
                FROM Sales.MiscMaster mode, Sales.MiscMaster method
                WHERE mode.Id = @FreightModeId AND mode.IsDeleted = 0 AND mode.IsActive = 1
                  AND method.Id = @RateMethodId AND method.IsDeleted = 0 AND method.IsActive = 1";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { FreightModeId = freightModeId, RateMethodId = rateMethodId });

            if (result == null)
                return false;

            string modeDesc = result.ModeDescription?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
            string methodDesc = result.MethodDescription?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;

            return modeDesc switch
            {
                "PER_KM" => methodDesc == "PER_KM",
                "INNER" => methodDesc is "PER_KG" or "PER_BAG" or "FIXED",
                "OUTER" => methodDesc is "PER_KG" or "PER_BAG" or "FIXED",
                _ => false
            };
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DispatchAddressMaster
                    WHERE FreightId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsFreightMasterLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DispatchAddressMaster
                    WHERE FreightId = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
