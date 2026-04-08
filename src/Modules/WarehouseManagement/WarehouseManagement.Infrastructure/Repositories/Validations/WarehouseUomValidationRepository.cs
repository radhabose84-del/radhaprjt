using System.Data;
using Contracts.Interfaces.Validations.WarehouseManagement;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Validations;

internal sealed class WarehouseUomValidationRepository : IWarehouseUomValidation
{
    private readonly IDbConnection _dbConnection;

    public WarehouseUomValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Warehouse].[WarehouseMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Warehouse].[RackMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Warehouse].[RackMaster] WHERE DimensionUOMId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Warehouse].[BinMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }

    public async Task<bool> HasActiveUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Warehouse].[WarehouseMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Warehouse].[RackMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Warehouse].[RackMaster] WHERE DimensionUOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Warehouse].[BinMaster] WHERE CapacityUOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }
}
