using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesChannel
{
    public class SalesChannelQueryRepository : ISalesChannelQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public SalesChannelQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<SalesChannelDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesChannel sc
                WHERE sc.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (sc.SalesChannelCode LIKE @Search OR sc.SalesChannelName LIKE @Search)")}};

                SELECT sc.Id, sc.SalesChannelCode, sc.SalesChannelName,
                    sc.IsActive, sc.IsDeleted,
                    sc.CreatedBy, sc.CreatedDate, sc.CreatedByName, sc.CreatedIP,
                    sc.ModifiedBy, sc.ModifiedDate, sc.ModifiedByName, sc.ModifiedIP
                FROM Sales.SalesChannel sc
                WHERE sc.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (sc.SalesChannelCode LIKE @Search OR sc.SalesChannelName LIKE @Search)")}}
                ORDER BY sc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesChannelDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<SalesChannelDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT sc.Id, sc.SalesChannelCode, sc.SalesChannelName,
                    sc.IsActive, sc.IsDeleted,
                    sc.CreatedBy, sc.CreatedDate, sc.CreatedByName, sc.CreatedIP,
                    sc.ModifiedBy, sc.ModifiedDate, sc.ModifiedByName, sc.ModifiedIP
                FROM Sales.SalesChannel sc
                WHERE sc.Id = @Id AND sc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesChannelDto>(sql, new { Id = id });

            return dto;
        }

        public async Task<bool> AlreadyExistsAsync(string salesChannelCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesChannel
                WHERE SalesChannelCode = @Code
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = salesChannelCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesChannel
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<IReadOnlyList<SalesChannelLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT  Id, SalesChannelCode, SalesChannelName
                FROM Sales.SalesChannel
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (SalesChannelCode LIKE @Term OR SalesChannelName LIKE @Term)
                ORDER BY SalesChannelName ASC";

            var result = await _dbConnection.QueryAsync<SalesChannelLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if SalesChannel is linked to active dependent records (blocking deletion).
            // Currently SalesChannel has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }
    }
}
