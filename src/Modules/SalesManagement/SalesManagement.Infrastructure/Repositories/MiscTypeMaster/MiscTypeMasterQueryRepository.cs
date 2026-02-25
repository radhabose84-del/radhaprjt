using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterQueryRepository : IMiscTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscTypeMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<MiscTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.MiscTypeMaster mtm
                WHERE mtm.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mtm.MiscTypeCode LIKE @Search OR mtm.Description LIKE @Search)")}};

                SELECT mtm.Id, mtm.MiscTypeCode, mtm.Description,
                    mtm.IsActive, mtm.IsDeleted,
                    mtm.CreatedBy, mtm.CreatedDate, mtm.CreatedByName, mtm.CreatedIP,
                    mtm.ModifiedBy, mtm.ModifiedDate, mtm.ModifiedByName, mtm.ModifiedIP
                FROM Sales.MiscTypeMaster mtm
                WHERE mtm.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mtm.MiscTypeCode LIKE @Search OR mtm.Description LIKE @Search)")}}
                ORDER BY mtm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<MiscTypeMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<MiscTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT mtm.Id, mtm.MiscTypeCode, mtm.Description,
                    mtm.IsActive, mtm.IsDeleted,
                    mtm.CreatedBy, mtm.CreatedDate, mtm.CreatedByName, mtm.CreatedIP,
                    mtm.ModifiedBy, mtm.ModifiedDate, mtm.ModifiedByName, mtm.ModifiedIP
                FROM Sales.MiscTypeMaster mtm
                WHERE mtm.Id = @Id AND mtm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<MiscTypeMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MiscTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, MiscTypeCode, Description
                FROM Sales.MiscTypeMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (MiscTypeCode LIKE @Term OR Description LIKE @Term)
                ORDER BY MiscTypeCode ASC";

            var result = await _dbConnection.QueryAsync<MiscTypeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscTypeMaster
                WHERE MiscTypeCode = @Code
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = miscTypeCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscTypeMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
