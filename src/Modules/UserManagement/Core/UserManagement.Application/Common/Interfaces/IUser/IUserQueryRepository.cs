using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IUser
{
    public interface IUserQueryRepository
    {
        Task<(List<User>, int)> GetAllUsersAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<User?> GetByIdAsync(int userId);
        Task<List<User>> GetUser(string searchPattern);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<User?> GetByUsernameAsync(string? username, int? id = null);
        Task<bool> AlreadyExistsAsync(string username, int? id = null);
        Task<bool> EmpIdAlreadyExistsAsync(int empId, int? id = null);
        Task<User?> GetByUserByUnit(int UserId, int UnitId);
        Task<(int UnitTypeId, string UnitTypeName)> GetUnitTypeByUnitIdAsync(int unitId);
        Task<bool> ValidateUsernameAsync(string? username, int? id = null);
        Task<bool> ValidateUserActiveAsync(string? username, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ValidateUserRolesAsync(string username);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> UserLockedAsync(string UserName);
        Task<bool> IsFirstimeUserValidation(string UserName);

        // Name lookups for DTO enrichment
        Task<Dictionary<int, string>> GetDepartmentNamesByIdsAsync(IEnumerable<int> departmentIds);
        Task<Dictionary<int, string>> GetUserGroupNamesByIdsAsync(IEnumerable<int> userGroupIds);
        Task<Dictionary<int, string>> GetCompanyNamesByIdsAsync(IEnumerable<int> companyIds);
        Task<Dictionary<int, string>> GetUnitNamesByIdsAsync(IEnumerable<int> unitIds);
        Task<Dictionary<int, string>> GetDivisionNamesByIdsAsync(IEnumerable<int> divisionIds);
        Task<Dictionary<int, string>> GetUserRoleNamesByIdsAsync(IEnumerable<int> roleIds);
        Task<Dictionary<int, string>> GetEntityNamesByIdsAsync(IEnumerable<int> entityIds);
        Task<Dictionary<int, string>> GetMarketingOfficerNamesByIdsAsync(IEnumerable<int> empIds);
    }

}