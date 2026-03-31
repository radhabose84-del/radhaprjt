using System.Data;
using Contracts.Interfaces.Validations.ProjectManagement;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.Validations;

internal sealed class ProjectDepartmentValidationRepository : IProjectDepartmentValidation
{
    private readonly IDbConnection _dbConnection;

    public ProjectDepartmentValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                 WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure] WHERE DepartmentId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }

    public async Task<bool> HasActiveDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                 WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure] WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }
}
