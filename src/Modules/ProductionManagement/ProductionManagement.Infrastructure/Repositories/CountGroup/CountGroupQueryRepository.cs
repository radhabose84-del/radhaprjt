using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Dto;

namespace ProductionManagement.Infrastructure.Repositories.CountGroup
{
    public class CountGroupQueryRepository : ICountGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CountGroupQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<CountGroupDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.CountGroup cg
                WHERE cg.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cg.CountGroupCode LIKE @SearchTerm
                       OR cg.CountGroupName LIKE @SearchTerm
                       OR cg.Description LIKE @SearchTerm);

                SELECT
                    cg.Id, cg.CountGroupCode, cg.CountGroupName, cg.Description,
                    cg.IsActive, cg.IsDeleted,
                    cg.CreatedBy, cg.CreatedDate, cg.CreatedByName, cg.CreatedIP,
                    cg.ModifiedBy, cg.ModifiedDate, cg.ModifiedByName, cg.ModifiedIP
                FROM Production.CountGroup cg
                WHERE cg.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cg.CountGroupCode LIKE @SearchTerm
                       OR cg.CountGroupName LIKE @SearchTerm
                       OR cg.Description LIKE @SearchTerm)
                ORDER BY cg.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<CountGroupDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<CountGroupDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    cg.Id, cg.CountGroupCode, cg.CountGroupName, cg.Description,
                    cg.IsActive, cg.IsDeleted,
                    cg.CreatedBy, cg.CreatedDate, cg.CreatedByName, cg.CreatedIP,
                    cg.ModifiedBy, cg.ModifiedDate, cg.ModifiedByName, cg.ModifiedIP
                FROM Production.CountGroup cg
                WHERE cg.Id = @Id AND cg.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<CountGroupDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<CountGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CountGroupCode, CountGroupName
                FROM Production.CountGroup
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (CountGroupCode LIKE @Term OR CountGroupName LIKE @Term)
                ORDER BY CountGroupName ASC";

            var result = await _dbConnection.QueryAsync<CountGroupLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string countGroupCode, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CountGroup
                WHERE CountGroupCode = @CountGroupCode AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CountGroupCode = countGroupCode, Id = id });
            return count > 0;
        }

        public async Task<bool> CountGroupNameExistsAsync(string countGroupName, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CountGroup
                WHERE CountGroupName = @CountGroupName AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CountGroupName = countGroupName, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CountGroup
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
