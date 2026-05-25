using System.Data;
using Contracts.Dtos.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class MaintenanceRequestValidationRepository : IMaintenanceRequestValidation
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceRequestValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> IsAvailableForServicePoAsync(int requestId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [Maintenance].[MaintenanceRequest] mr
                INNER JOIN [Maintenance].[MiscMaster] rt ON mr.RequestTypeId = rt.Id AND rt.IsDeleted = 0
                INNER JOIN [Maintenance].[MiscMaster] rs ON mr.RequestStatusId = rs.Id AND rs.IsDeleted = 0
                WHERE mr.Id = @Id
                  AND mr.IsDeleted = 0
                  AND mr.IsActive = 1
                  AND rt.Code = 'External'
                  AND rs.Code IN ('Open', 'InProgress', 'PartiallyConverted')
            ) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END";

        return await _dbConnection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Id = requestId }, cancellationToken: ct));
    }

    public async Task<MaintenanceRequestRefDto?> GetRefAsync(int requestId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TOP 1
                mr.Id,
                mr.VendorId,
                mr.ServiceTypeId,
                mr.RequestStatusId,
                rs.Code AS RequestStatusCode
            FROM [Maintenance].[MaintenanceRequest] mr
            LEFT JOIN [Maintenance].[MiscMaster] rs ON mr.RequestStatusId = rs.Id AND rs.IsDeleted = 0
            WHERE mr.Id = @Id AND mr.IsDeleted = 0;";

        return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceRequestRefDto>(
            new CommandDefinition(sql, new { Id = requestId }, cancellationToken: ct));
    }
}
