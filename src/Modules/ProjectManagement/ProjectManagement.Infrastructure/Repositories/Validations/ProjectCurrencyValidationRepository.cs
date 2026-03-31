using System.Data;
using Contracts.Interfaces.Validations.ProjectManagement;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.Validations;

internal sealed class ProjectCurrencyValidationRepository : IProjectCurrencyValidation
{
    private readonly IDbConnection _dbConnection;

    public ProjectCurrencyValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedCurrencyAsync(int currencyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                  WHERE CurrencyId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure]  WHERE CurrencyId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
    }
}
