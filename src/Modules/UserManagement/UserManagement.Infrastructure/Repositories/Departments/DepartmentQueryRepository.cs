#nullable disable
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IDepartment;
using System.Data;
using Dapper;
using UserManagement.Application.Departments.Queries.GetDepartments;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.BudgetManagement;
using Contracts.Interfaces.Validations.ProjectManagement;
using Contracts.Interfaces.Validations.WarehouseManagement;

namespace UserManagement.Infrastructure.Repositories.Departments
{
    public class DepartmentQueryRepository : IDepartmentQueryRepository
  {
    private readonly IDbConnection _dbConnection;
    private readonly IIPAddressService _ipAddressService;
    private readonly IMaintenanceDepartmentValidation _maintenanceDeptValidation;
    private readonly IBudgetDepartmentValidation _budgetDeptValidation;
    private readonly IProjectDepartmentValidation _projectDeptValidation;
    private readonly IWarehouseDepartmentValidation _warehouseDeptValidation;

    public DepartmentQueryRepository(
        IDbConnection dbConnection,
        IIPAddressService ipAddressService,
        IMaintenanceDepartmentValidation maintenanceDeptValidation,
        IBudgetDepartmentValidation budgetDeptValidation,
        IProjectDepartmentValidation projectDeptValidation,
        IWarehouseDepartmentValidation warehouseDeptValidation)
    {
      _dbConnection = dbConnection;
      _ipAddressService = ipAddressService;
      _maintenanceDeptValidation = maintenanceDeptValidation;
      _budgetDeptValidation = budgetDeptValidation;
      _projectDeptValidation = projectDeptValidation;
      _warehouseDeptValidation = warehouseDeptValidation;
    }
   

    public async Task<(List<DepartmentDto>, int)> GetAllDepartmentAsync(int PageNumber, int PageSize, string SearchTerm)
    {
      
      var query = $$"""
            DECLARE @TotalCount INT;

            SELECT @TotalCount = COUNT(*)
            FROM AppData.Department D
            INNER JOIN AppData.DepartmentGroup DG ON DG.Id = D.DepartmentGroupId
            WHERE D.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (D.ShortName LIKE @Search OR D.DeptName LIKE @Search OR DG.DepartmentGroupName LIKE @Search)")}};

            SELECT 
                D.Id,
                D.CompanyId,
                D.ShortName,
                D.DeptName,
                D.DepartmentGroupId,
                DG.DepartmentGroupName AS DepartmentGroupName, 
                D.CreatedIP,
                D.IsActive,
                D.CreatedBy,
                D.CreatedByName,
                D.CreatedAt,
                D.ModifiedBy,
                D.ModifiedByName,
                D.ModifiedAt,
                D.ModifiedIP,
                D.IsDeleted
            FROM AppData.Department D
            INNER JOIN AppData.DepartmentGroup DG ON DG.Id = D.DepartmentGroupId
            WHERE D.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (D.ShortName LIKE @Search OR D.DeptName LIKE @Search OR DG.DepartmentGroupName LIKE @Search)")}}
            ORDER BY D.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
        """;

      var parameters = new
      {
        Search = $"%{SearchTerm}%",
        Offset = (PageNumber - 1) * PageSize,
        PageSize
      };

      var result = await _dbConnection.QueryMultipleAsync(query, parameters);
      var departmentList = (await result.ReadAsync<DepartmentDto>()).ToList(); // map to DTO if needed
      int totalCount = await result.ReadFirstAsync<int>();

      return (departmentList, totalCount);
    }



    public async Task<Department> GetByIdAsync(int id)
    {
      const string query = @"SELECT * FROM AppData.Department WHERE Id = @Id AND IsDeleted = 0 ORDER BY Id DESC ";
      var departments = await _dbConnection.QueryAsync<Department>(query, new { Id = id });

      var department = departments.FirstOrDefault();

      if (department == null)
      {
        return null;
      }

      return department; // Returns null if not found

    }

    public async Task<List<DepartmentWithGroupDto>> GetDepartmentsByDepartmentGroupIdAsync(string departmentGroupName)
    {
      const string query = @"
            SELECT 
                D.Id,
                D.CompanyId,
                D.ShortName,
                D.DeptName,
                D.DepartmentGroupId,
                DG.DepartmentGroupName,
                D.IsActive
            FROM AppData.Department D           
            INNER JOIN AppData.DepartmentGroup DG ON DG.Id = D.DepartmentGroupId
            WHERE DG.DepartmentGroupName = @DepartmentGroupName AND D.IsDeleted = 0 AND D.IsActive = 1";

      var departments = await _dbConnection.QueryAsync<DepartmentWithGroupDto>(query, new { DepartmentGroupName = departmentGroupName });

      return departments.ToList();
    }


    public async Task<List<Department>> GetAllDepartmentAutoCompleteSearchAsync(string SearchDept)
    {
      var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
      var userId = _ipAddressService.GetUserId();
      const string query = @"
            SELECT D.Id,D.CompanyId,D.ShortName,D.DeptName,D.DepartmentGroupId ,DG.DepartmentGroupName,D.IsActive FROM  AppData.Department D
            INNER JOIN [AppSecurity].[UserDepartment] UD ON UD.DepartmentId=D.Id AND UD.IsActive=1
            INNER JOIN [AppData].[DepartmentGroup] DG ON DG.Id=D.DepartmentGroupId
            WHERE (D.DeptName LIKE @SearchDept OR  D.ShortName LIKE @SearchDept) and D.IsDeleted = 0 AND D.CompanyId=@CompanyId AND UD.UserId=@UserId
            ORDER BY D.Id DESC";

      var parameters = new
      {
        SearchDept = $"%{SearchDept ?? string.Empty}%",
        CompanyId = CompanyId,
        UserId = userId
      };

      var departments = await _dbConnection.QueryAsync<Department>(query, parameters);
      return departments.ToList();
    }

    public async Task<bool> FKColumnExistValidation(int Id)
    {
      var sql = "SELECT COUNT(1) FROM AppData.Department WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
      var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
      return count > 0;
    }
    public async Task<List<Department>> GetDepartment_SuperAdmin(string SearchDept)
    {
      var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
      const string query = @"
            SELECT D.Id,D.CompanyId,D.ShortName,D.DeptName,D.IsActive FROM  AppData.Department D
            WHERE (D.DeptName LIKE @SearchDept OR  D.ShortName LIKE @SearchDept) and D.IsDeleted = 0 AND D.CompanyId=@CompanyId 
            ORDER BY D.Id DESC";

      var parameters = new
      {
        SearchDept = $"%{SearchDept ?? string.Empty}%",
        CompanyId = CompanyId
      };

      var departments = await _dbConnection.QueryAsync<Department>(query, parameters);
      return departments.ToList();
    }

    public async Task<List<DepartmentWithControlByGroupDto>> GetDepartmentsByDepartmentGroupWithControl(string departmentGroupName)
    {
      var userId = _ipAddressService.GetUserId();
      const string query = @"
                 SELECT 
                     D.Id,
                     D.ShortName,
                     D.DeptName,
                     D.DepartmentGroupId,
                     DG.DepartmentGroupName
                 FROM AppData.Department D           
                 INNER JOIN AppData.DepartmentGroup DG ON DG.Id = D.DepartmentGroupId
                 INNER JOIN [AppSecurity].[UserDepartment] UD ON UD.DepartmentId = D.Id
                 WHERE DG.DepartmentGroupName = @DepartmentGroupName AND D.IsDeleted = 0 AND D.IsActive = 1 AND UD.UserId =@UserId AND UD.IsActive=1";

      var departments = await _dbConnection.QueryAsync<DepartmentWithControlByGroupDto>(query, new { DepartmentGroupName = departmentGroupName, UserId = userId });

      return departments.ToList();
    }

    public async Task<bool> IsDepartmentUsedByAnyUserAsync(int departmentId)
    {
      const string sql = @"
        SELECT COUNT(1)
        FROM AppSecurity.UserDepartment UD
        WHERE UD.DepartmentId = @DepartmentId
          AND UD.IsActive = 1";

      var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { DepartmentId = departmentId });
      return count > 0;
    }

    public async Task<bool> IsDepartmentLinkedAsync(int departmentId)
    {
      // Same-module: check UserDepartment
      const string userDeptQuery = @"
        SELECT CASE WHEN
            EXISTS (SELECT 1 FROM [AppSecurity].[UserDepartment] WHERE DepartmentId = @departmentId AND IsActive = 1)
        THEN 1 ELSE 0 END";

      var userLinked = await _dbConnection.ExecuteScalarAsync<bool>(userDeptQuery, new { departmentId });
      if (userLinked) return true;

      // Cross-module checks via validation interfaces
      if (await _maintenanceDeptValidation.HasActiveDepartmentAsync(departmentId)) return true;
      if (await _budgetDeptValidation.HasActiveDepartmentAsync(departmentId)) return true;
      if (await _projectDeptValidation.HasActiveDepartmentAsync(departmentId)) return true;
      if (await _warehouseDeptValidation.HasActiveDepartmentAsync(departmentId)) return true;

      return false;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
      // Same-module: check UserDepartment
      const string sql = @"
        SELECT CASE WHEN
            EXISTS (SELECT 1 FROM [AppSecurity].[UserDepartment] WHERE DepartmentId = @Id AND IsActive = 1)
        THEN 1 ELSE 0 END";

      var inModule = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
      if (inModule) return true;

      // Cross-module checks via validation interfaces
      if (await _maintenanceDeptValidation.HasLinkedDepartmentAsync(id)) return true;
      if (await _budgetDeptValidation.HasLinkedDepartmentAsync(id)) return true;
      if (await _projectDeptValidation.HasLinkedDepartmentAsync(id)) return true;
      if (await _warehouseDeptValidation.HasLinkedDepartmentAsync(id)) return true;

      return false;
    }
  }
}