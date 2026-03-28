using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.YarnTwistMaster
{
    public class YarnTwistMasterQueryRepository : IYarnTwistMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public YarnTwistMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<YarnTwistMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.YarnTwistMaster ytm
                WHERE ytm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR ytm.TwistName LIKE @SearchTerm
                       OR ytm.Description LIKE @SearchTerm);

                SELECT
                    ytm.Id, ytm.TwistName, ytm.Description,
                    ytm.IsActive, ytm.IsDeleted,
                    ytm.CreatedBy, ytm.CreatedDate, ytm.CreatedByName, ytm.CreatedIP,
                    ytm.ModifiedBy, ytm.ModifiedDate, ytm.ModifiedByName, ytm.ModifiedIP
                FROM Production.YarnTwistMaster ytm
                WHERE ytm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR ytm.TwistName LIKE @SearchTerm
                       OR ytm.Description LIKE @SearchTerm)
                ORDER BY ytm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<YarnTwistMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<YarnTwistMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    ytm.Id, ytm.TwistName, ytm.Description,
                    ytm.IsActive, ytm.IsDeleted,
                    ytm.CreatedBy, ytm.CreatedDate, ytm.CreatedByName, ytm.CreatedIP,
                    ytm.ModifiedBy, ytm.ModifiedDate, ytm.ModifiedByName, ytm.ModifiedIP
                FROM Production.YarnTwistMaster ytm
                WHERE ytm.Id = @Id AND ytm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<YarnTwistMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<YarnTwistMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, TwistName
                FROM Production.YarnTwistMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND TwistName LIKE @Term
                ORDER BY TwistName ASC";

            var result = await _dbConnection.QueryAsync<YarnTwistMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> TwistNameExistsAsync(string twistName, int? excludeId = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnTwistMaster
                WHERE TwistName = @TwistName AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TwistName = twistName, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.YarnTwistMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
