using System.Data;
using Contracts.Interfaces.Validations.WarehouseManagement;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Validations;

internal sealed class WarehouseItemGroupValidationRepository : IWarehouseItemGroupValidation
{
    private readonly IDbConnection _dbConnection;

    public WarehouseItemGroupValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemGroupAsync(int itemGroupId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Warehouse].[WarehouseItemGroupMapping]
                WHERE ItemGroupId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemGroupId });
    }

    public async Task<bool> HasActiveItemGroupAsync(int itemGroupId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Warehouse].[WarehouseItemGroupMapping]
                WHERE ItemGroupId = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemGroupId });
    }
}
