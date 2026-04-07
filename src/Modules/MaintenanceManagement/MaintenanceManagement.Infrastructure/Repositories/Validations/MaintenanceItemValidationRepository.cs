using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class MaintenanceItemValidationRepository : IMaintenanceItemValidation
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceItemValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[PreventiveSchedulerItems]
                WHERE ItemId = @Id
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }

    public async Task<bool> HasActiveItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[PreventiveSchedulerItems]
                WHERE ItemId = @Id
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }
}
