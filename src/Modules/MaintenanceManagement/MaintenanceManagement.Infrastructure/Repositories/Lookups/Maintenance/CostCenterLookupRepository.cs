using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;

namespace MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance
{
    internal class CostCenterLookupRepository : ICostCenterLookup
    {
        private readonly IDbConnection _dbConnection;

        public CostCenterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CostCenterLookupDto?> GetByIdAsync(int costCenterId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    Id AS CostCenterId,
                    CostCenterCode,
                    CostCenterName,
                    UnitId,
                    DepartmentId
                FROM [Maintenance].[CostCenter]
                WHERE IsDeleted = 0 AND Id = @CostCenterId;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<CostCenterLookupDto>(
                new CommandDefinition(sql, new { CostCenterId = costCenterId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<CostCenterLookupDto>> GetByIdsAsync(IEnumerable<int> costCenterIds, CancellationToken ct = default)
        {
            var ids = costCenterIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0)
                return Array.Empty<CostCenterLookupDto>();

            const string sql = @"
                SELECT
                    Id AS CostCenterId,
                    CostCenterCode,
                    CostCenterName,
                    UnitId,
                    DepartmentId
                FROM [Maintenance].[CostCenter]
                WHERE IsDeleted = 0 AND Id IN @Ids;
            ";

            var rows = await _dbConnection.QueryAsync<CostCenterLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return rows.ToList();
        }

        public async Task<List<CostCenterLookupDto>> GetAllCostCentersAsync()
        {
            const string sql = @"
                SELECT
                    Id AS CostCenterId,
                    CostCenterCode,
                    CostCenterName,
                    UnitId,
                    DepartmentId
                FROM [Maintenance].[CostCenter]
                WHERE IsDeleted = 0 AND IsActive = 1
                ORDER BY CostCenterName ASC;
            ";

            var result = await _dbConnection.QueryAsync<CostCenterLookupDto>(sql);
            return result.ToList();
        }
    }
}
