using System.Data;
using Contracts.Interfaces.Validations.BudgetManagement;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.Validations;

internal sealed class BudgetCurrencyValidationRepository : IBudgetCurrencyValidation
{
    private readonly IDbConnection _dbConnection;

    public BudgetCurrencyValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedCurrencyAsync(int currencyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Budget].[BudgetGroup]   WHERE CurrencyId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE CurrencyId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
    }
}
