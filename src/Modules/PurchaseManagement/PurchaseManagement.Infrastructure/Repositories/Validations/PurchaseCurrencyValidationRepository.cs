using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PurchaseCurrencyValidationRepository : IPurchaseCurrencyValidation
{
    private readonly IDbConnection _dbConnection;

    public PurchaseCurrencyValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedCurrencyAsync(int currencyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE CurrencyId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[PriceMasterDetail]    WHERE CurrencyId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail]      WHERE CurrencyId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
    }
}
