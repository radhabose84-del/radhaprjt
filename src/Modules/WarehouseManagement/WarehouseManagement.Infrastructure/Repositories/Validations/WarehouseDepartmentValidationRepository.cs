using System.Data;
using Contracts.Interfaces.Validations.WarehouseManagement;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Validations;

internal sealed class WarehouseDepartmentValidationRepository : IWarehouseDepartmentValidation
{
    private readonly IDbConnection _dbConnection;

    public WarehouseDepartmentValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Warehouse].[WarehouseMaster] WHERE DepartmentId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }

    public async Task<bool> HasActiveDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Warehouse].[WarehouseMaster] WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }
}
