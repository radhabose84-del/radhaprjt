using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class SalesCurrencyValidationRepository : ISalesCurrencyValidation
{
    private readonly IDbConnection _dbConnection;

    public SalesCurrencyValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedCurrencyAsync(int currencyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[ItemPriceMaster] WHERE CurrencyId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Sales].[SalesSegment]    WHERE CurrencyId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
    }
}
