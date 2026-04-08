using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PurchaseHsnValidationRepository : IPurchaseHsnValidation
{
    private readonly IDbConnection _dbConnection;

    public PurchaseHsnValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedHsnAsync(int hsnId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE HsnId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE HsnId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = hsnId });
    }

    public async Task<bool> HasActiveHsnAsync(int hsnId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE HsnId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE HsnId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = hsnId });
    }
}
