using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class PartyMasterMaintenanceValidationRepository : IPartyMasterMaintenanceValidation
{
    private readonly IDbConnection _dbConnection;

    public PartyMasterMaintenanceValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedPartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MaintenanceRequest] WHERE VendorId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }

    public async Task<bool> HasActivePartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Maintenance].[MaintenanceRequest] WHERE VendorId = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }
}
