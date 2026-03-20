using System.Data;
using Contracts.Interfaces.Lookups.Maintenance;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance
{
    internal class DepartmentValidationLookupRepository : IDepartmentValidationLookup
    {
        private readonly IDbConnection _dbConnection;

        public DepartmentValidationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> IsDepartmentUsedAsync(int departmentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1 FROM Maintenance.CostCenter WHERE DepartmentId = @DepartmentId
                    )
                    THEN 1
                    ELSE 0
                END;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { DepartmentId = departmentId }, cancellationToken: ct));

            return result == 1;
        }
    }
}
