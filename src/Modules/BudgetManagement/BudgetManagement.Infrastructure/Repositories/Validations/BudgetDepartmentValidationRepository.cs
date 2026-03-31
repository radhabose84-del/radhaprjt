using System.Data;
using Contracts.Interfaces.Validations.BudgetManagement;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.Validations;

internal sealed class BudgetDepartmentValidationRepository : IBudgetDepartmentValidation
{
    private readonly IDbConnection _dbConnection;

    public BudgetDepartmentValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE DepartmentId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }

    public async Task<bool> HasActiveDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }
}
