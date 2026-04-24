using System.Data;
using Contracts.Interfaces.Validations.ProductionManagement;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Validations;

internal sealed class ProductionItemValidationRepository : IProductionItemValidation
{
    private readonly IDbConnection _dbConnection;

    public ProductionItemValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE OldItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Production].[LooseConeLedger] WHERE ItemId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }

    public async Task<bool> HasActiveItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE OldItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Production].[LooseConeLedger] WHERE ItemId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }
}
