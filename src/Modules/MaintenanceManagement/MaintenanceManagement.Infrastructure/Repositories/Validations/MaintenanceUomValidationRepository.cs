using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class MaintenanceUomValidationRepository : IMaintenanceUomValidation
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceUomValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MachineMaster]
                WHERE UomId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }

    public async Task<bool> HasActiveUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MachineMaster]
                WHERE UomId = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }
}
