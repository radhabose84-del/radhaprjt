using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.CertificationMaster
{
    public class CertificationMasterQueryRepository : ICertificationMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CertificationMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<CertificationMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.CertificationMaster cm
                WHERE cm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cm.CertificationName LIKE @SearchTerm
                       OR cm.Description LIKE @SearchTerm);

                SELECT
                    cm.Id, cm.CertificationName, cm.Description,
                    cm.IsActive, cm.IsDeleted,
                    cm.CreatedBy, cm.CreatedDate, cm.CreatedByName, cm.CreatedIP,
                    cm.ModifiedBy, cm.ModifiedDate, cm.ModifiedByName, cm.ModifiedIP
                FROM Production.CertificationMaster cm
                WHERE cm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cm.CertificationName LIKE @SearchTerm
                       OR cm.Description LIKE @SearchTerm)
                ORDER BY cm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<CertificationMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<CertificationMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    cm.Id, cm.CertificationName, cm.Description,
                    cm.IsActive, cm.IsDeleted,
                    cm.CreatedBy, cm.CreatedDate, cm.CreatedByName, cm.CreatedIP,
                    cm.ModifiedBy, cm.ModifiedDate, cm.ModifiedByName, cm.ModifiedIP
                FROM Production.CertificationMaster cm
                WHERE cm.Id = @Id AND cm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<CertificationMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<CertificationMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CertificationName
                FROM Production.CertificationMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND CertificationName LIKE @Term
                ORDER BY CertificationName ASC";

            var result = await _dbConnection.QueryAsync<CertificationMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> CertificationNameExistsAsync(string certificationName, int? excludeId = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CertificationMaster
                WHERE CertificationName = @CertificationName AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CertificationName = certificationName, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CertificationMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
