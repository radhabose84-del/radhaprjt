using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;

namespace SalesManagement.Infrastructure.Repositories.CommissionSplit
{
    public class CommissionSplitQueryRepository : ICommissionSplitQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CommissionSplitQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<CommissionSplitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            const string countSql = @"
                SELECT COUNT(*)
                FROM Sales.CommissionSplit cs
                WHERE cs.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR cs.SplitName LIKE '%' + @SearchTerm + '%'
                       OR cs.SplitCode LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT
                    cs.Id, cs.SplitCode, cs.SplitName,
                    cs.IsActive, cs.IsDeleted,
                    cs.CreatedBy, cs.CreatedDate, cs.CreatedByName, cs.CreatedIP,
                    cs.ModifiedBy, cs.ModifiedDate, cs.ModifiedByName, cs.ModifiedIP
                FROM Sales.CommissionSplit cs
                WHERE cs.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR cs.SplitName LIKE '%' + @SearchTerm + '%'
                       OR cs.SplitCode LIKE '%' + @SearchTerm + '%')
                ORDER BY cs.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<CommissionSplitDto>(dataSql, parameters)).ToList();

            return (data, totalCount);
        }

        public async Task<CommissionSplitDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    cs.Id, cs.SplitCode, cs.SplitName,
                    cs.IsActive, cs.IsDeleted,
                    cs.CreatedBy, cs.CreatedDate, cs.CreatedByName, cs.CreatedIP,
                    cs.ModifiedBy, cs.ModifiedDate, cs.ModifiedByName, cs.ModifiedIP
                FROM Sales.CommissionSplit cs
                WHERE cs.Id = @Id AND cs.IsDeleted = 0";

            const string detailsSql = @"
                SELECT
                    d.Id, d.RoleId, r.Description AS RoleName,
                    d.ShareTypeId, st.Description AS ShareTypeName,
                    d.ShareValue
                FROM Sales.CommissionSplitDetail d
                LEFT JOIN Sales.MiscMaster r ON d.RoleId = r.Id AND r.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON d.ShareTypeId = st.Id AND st.IsDeleted = 0
                WHERE d.CommissionSplitId = @Id AND d.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CommissionSplitDto>(headerSql, new { Id = id });

            if (dto == null)
                return null;

            dto.Details = (await _dbConnection.QueryAsync<CommissionSplitDetailDto>(detailsSql, new { Id = id })).ToList();

            return dto;
        }

        public async Task<IReadOnlyList<CommissionSplitLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, SplitCode, SplitName
                FROM Sales.CommissionSplit
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term = '' OR SplitName LIKE '%' + @Term + '%'
                       OR SplitCode LIKE '%' + @Term + '%')
                ORDER BY SplitName ASC";

            var result = await _dbConnection.QueryAsync<CommissionSplitLookupDto>(sql, new { Term = term });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string splitName, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.CommissionSplit
                    WHERE SplitName = @SplitName AND IsDeleted = 0
                      AND (@Id IS NULL OR Id <> @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { SplitName = splitName, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Sales.CommissionSplit
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

        public async Task<string?> GetMiscMasterCodeAsync(int id)
        {
            const string sql = @"
                SELECT Code FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = id });
        }
    }
}
