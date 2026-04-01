using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class MaintenanceManufacturerValidationRepository : IMaintenanceManufacturerValidation
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceManufacturerValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedManufacturerAsync(int manufacturerId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MachineGroup]
                WHERE [Manufacturer] = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = manufacturerId });
    }

    public async Task<bool> HasActiveManufacturerAsync(int manufacturerId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MachineGroup]
                WHERE [Manufacturer] = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = manufacturerId });
    }
}
