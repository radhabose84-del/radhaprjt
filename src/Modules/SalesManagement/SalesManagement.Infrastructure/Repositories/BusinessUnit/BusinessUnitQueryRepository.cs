
using Dapper;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using System.Data;

namespace SalesManagement.Infrastructure.Repositories.BusinessUnit
{
    public class BusinessUnitQueryRepository : IBusinessUnitQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public BusinessUnitQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<BusinessUnitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause += " AND (BusinessUnitCode LIKE @SearchTerm OR BusinessUnitName LIKE @SearchTerm)";
            }

            var sql = $@"
                SELECT * FROM Sales.BusinessUnit
                {whereClause}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM Sales.BusinessUnit {whereClause};
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            });

            var data = (await multi.ReadAsync<BusinessUnitDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (data, totalCount);
        }

        public async Task<BusinessUnitDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT * FROM Sales.BusinessUnit
                WHERE Id = @Id AND IsDeleted = 0
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<BusinessUnitDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<BusinessUnitLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken)
        {
            var sql = @"
                SELECT Id, BusinessUnitCode, BusinessUnitName
                FROM Sales.BusinessUnit
                WHERE IsActive = 1 AND IsDeleted = 0
            ";

            if (!string.IsNullOrWhiteSpace(term))
            {
                sql += " AND (BusinessUnitCode LIKE @Term OR BusinessUnitName LIKE @Term)";
            }

            sql += " ORDER BY BusinessUnitName";

            var result = await _dbConnection.QueryAsync<BusinessUnitLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string businessUnitCode, int? id = null)
        {
            var sql = "SELECT COUNT(1) FROM Sales.BusinessUnit WHERE BusinessUnitCode = @BusinessUnitCode AND IsDeleted = 0";

            if (id.HasValue)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { BusinessUnitCode = businessUnitCode, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.BusinessUnit WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if BusinessUnit is linked to active dependent records (blocking deletion).
            // Currently BusinessUnit has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }
    }
}
