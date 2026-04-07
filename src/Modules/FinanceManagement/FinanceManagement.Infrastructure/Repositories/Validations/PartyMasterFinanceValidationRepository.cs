using System.Data;
using Contracts.Interfaces.Validations.FinanceManagement;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Validations;

internal sealed class PartyMasterFinanceValidationRepository : IPartyMasterFinanceValidation
{
    private readonly IDbConnection _dbConnection;

    public PartyMasterFinanceValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedPartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Finance].[EInvoiceHeader] WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Finance].[EWaybillHeader] WHERE PartyId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }

    public async Task<bool> HasActivePartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Finance].[EInvoiceHeader] WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Finance].[EWaybillHeader] WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }
}
