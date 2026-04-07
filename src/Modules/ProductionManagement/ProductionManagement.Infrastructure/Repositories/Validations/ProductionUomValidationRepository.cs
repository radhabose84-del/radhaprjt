using System.Data;
using Contracts.Interfaces.Validations.ProductionManagement;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Validations;

internal sealed class ProductionUomValidationRepository : IProductionUomValidation
{
    private readonly IDbConnection _dbConnection;

    public ProductionUomValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Production].[CountMaster]
                WHERE UOMId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }

    public async Task<bool> HasActiveUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Production].[CountMaster]
                WHERE UOMId = @Id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }
}
