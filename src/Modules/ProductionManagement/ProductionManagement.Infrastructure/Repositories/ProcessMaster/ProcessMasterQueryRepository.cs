using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.ProcessMaster
{
    public class ProcessMasterQueryRepository : IProcessMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ProcessMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<ProcessMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.ProcessMaster pm
                WHERE pm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR pm.ProcessName LIKE @SearchTerm
                       OR pm.Description LIKE @SearchTerm);

                SELECT
                    pm.Id, pm.ProcessName, pm.CombingRequired, pm.Description,
                    pm.IsActive, pm.IsDeleted,
                    pm.CreatedBy, pm.CreatedDate, pm.CreatedByName, pm.CreatedIP,
                    pm.ModifiedBy, pm.ModifiedDate, pm.ModifiedByName, pm.ModifiedIP
                FROM Production.ProcessMaster pm
                WHERE pm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR pm.ProcessName LIKE @SearchTerm
                       OR pm.Description LIKE @SearchTerm)
                ORDER BY pm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<ProcessMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<ProcessMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    pm.Id, pm.ProcessName, pm.CombingRequired, pm.Description,
                    pm.IsActive, pm.IsDeleted,
                    pm.CreatedBy, pm.CreatedDate, pm.CreatedByName, pm.CreatedIP,
                    pm.ModifiedBy, pm.ModifiedDate, pm.ModifiedByName, pm.ModifiedIP
                FROM Production.ProcessMaster pm
                WHERE pm.Id = @Id AND pm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<ProcessMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<ProcessMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ProcessName
                FROM Production.ProcessMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND ProcessName LIKE @Term
                ORDER BY ProcessName ASC";

            var result = await _dbConnection.QueryAsync<ProcessMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> ProcessNameExistsAsync(string processName, int? excludeId = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.ProcessMaster
                WHERE ProcessName = @ProcessName AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ProcessName = processName, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.ProcessMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
