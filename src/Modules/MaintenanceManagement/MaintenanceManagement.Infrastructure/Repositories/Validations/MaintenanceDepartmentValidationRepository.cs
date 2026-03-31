using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Validations;

internal sealed class MaintenanceDepartmentValidationRepository : IMaintenanceDepartmentValidation
{
    private readonly IDbConnection _dbConnection;

    public MaintenanceDepartmentValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Maintenance].[CostCenter]                WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[MachineGroup]              WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[Feeder]                    WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[WorkCenter]                WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[PreventiveSchedulerHeader] WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[ActivityMaster]            WHERE DepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[MaintenanceRequest]        WHERE MaintenanceDepartmentId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[MaintenanceRequest]        WHERE ProductionDepartmentId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }

    public async Task<bool> HasActiveDepartmentAsync(int departmentId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Maintenance].[CostCenter]                WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[MachineGroup]              WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[Feeder]                    WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[WorkCenter]                WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[PreventiveSchedulerHeader] WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR
                EXISTS (SELECT 1 FROM [Maintenance].[ActivityMaster]            WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
    }
}
