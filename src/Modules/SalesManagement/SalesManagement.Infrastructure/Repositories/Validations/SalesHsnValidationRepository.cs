using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class SalesHsnValidationRepository : ISalesHsnValidation
{
    private readonly IDbConnection _dbConnection;

    public SalesHsnValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedHsnAsync(int hsnId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE HSNId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationDetail] WHERE HSNId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = hsnId });
    }

    public async Task<bool> HasActiveHsnAsync(int hsnId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE HSNId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationDetail] WHERE HSNId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = hsnId });
    }
}
