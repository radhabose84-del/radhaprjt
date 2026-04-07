using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PurchaseItemCategoryValidationRepository : IPurchaseItemCategoryValidation
{
    private readonly IDbConnection _dbConnection;

    public PurchaseItemCategoryValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemCategoryAsync(int itemCategoryId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Purchase].[IndentDetail]
                WHERE ItemCategoryId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemCategoryId });
    }

    public async Task<bool> HasActiveItemCategoryAsync(int itemCategoryId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Purchase].[IndentDetail]
                WHERE ItemCategoryId = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemCategoryId });
    }
}
