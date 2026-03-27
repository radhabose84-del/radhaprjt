using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.QualityMaster
{
    public class QualityMasterQueryRepository : IQualityMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public QualityMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<QualityMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.QualityMaster qm
                WHERE qm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR qm.QualityName LIKE @SearchTerm
                       OR qm.Description LIKE @SearchTerm);

                SELECT
                    qm.Id, qm.QualityName, qm.Description,
                    qm.IsActive, qm.IsDeleted,
                    qm.CreatedBy, qm.CreatedDate, qm.CreatedByName, qm.CreatedIP,
                    qm.ModifiedBy, qm.ModifiedDate, qm.ModifiedByName, qm.ModifiedIP
                FROM Production.QualityMaster qm
                WHERE qm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR qm.QualityName LIKE @SearchTerm
                       OR qm.Description LIKE @SearchTerm)
                ORDER BY qm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<QualityMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<QualityMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    qm.Id, qm.QualityName, qm.Description,
                    qm.IsActive, qm.IsDeleted,
                    qm.CreatedBy, qm.CreatedDate, qm.CreatedByName, qm.CreatedIP,
                    qm.ModifiedBy, qm.ModifiedDate, qm.ModifiedByName, qm.ModifiedIP
                FROM Production.QualityMaster qm
                WHERE qm.Id = @Id AND qm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<QualityMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<QualityMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, QualityName
                FROM Production.QualityMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND QualityName LIKE @Term
                ORDER BY QualityName ASC";

            var result = await _dbConnection.QueryAsync<QualityMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> QualityNameExistsAsync(string qualityName, int? excludeId = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.QualityMaster
                WHERE QualityName = @QualityName AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { QualityName = qualityName, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.QualityMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
