using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Dto;

namespace ProductionManagement.Infrastructure.Repositories.YarnType
{
    public class YarnTypeQueryRepository : IYarnTypeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public YarnTypeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<YarnTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.YarnType yt
                WHERE yt.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR yt.YarnTypeCode LIKE @SearchTerm
                       OR yt.YarnTypeName LIKE @SearchTerm
                       OR yt.Description LIKE @SearchTerm);

                SELECT
                    yt.Id, yt.YarnTypeCode, yt.YarnTypeName, yt.Description,
                    yt.IsActive, yt.IsDeleted,
                    yt.CreatedBy, yt.CreatedDate, yt.CreatedByName, yt.CreatedIP,
                    yt.ModifiedBy, yt.ModifiedDate, yt.ModifiedByName, yt.ModifiedIP
                FROM Production.YarnType yt
                WHERE yt.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR yt.YarnTypeCode LIKE @SearchTerm
                       OR yt.YarnTypeName LIKE @SearchTerm
                       OR yt.Description LIKE @SearchTerm)
                ORDER BY yt.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<YarnTypeDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<YarnTypeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    yt.Id, yt.YarnTypeCode, yt.YarnTypeName, yt.Description,
                    yt.IsActive, yt.IsDeleted,
                    yt.CreatedBy, yt.CreatedDate, yt.CreatedByName, yt.CreatedIP,
                    yt.ModifiedBy, yt.ModifiedDate, yt.ModifiedByName, yt.ModifiedIP
                FROM Production.YarnType yt
                WHERE yt.Id = @Id AND yt.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<YarnTypeDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<YarnTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, YarnTypeCode, YarnTypeName
                FROM Production.YarnType
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (YarnTypeCode LIKE @Term OR YarnTypeName LIKE @Term)
                ORDER BY YarnTypeName ASC";

            var result = await _dbConnection.QueryAsync<YarnTypeLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string yarnTypeCode, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnType
                WHERE YarnTypeCode = @YarnTypeCode AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { YarnTypeCode = yarnTypeCode, Id = id });
            return count > 0;
        }

        public async Task<bool> YarnTypeNameExistsAsync(string yarnTypeName, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnType
                WHERE YarnTypeName = @YarnTypeName AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { YarnTypeName = yarnTypeName, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnType
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
