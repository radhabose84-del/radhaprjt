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
        Task<User?> GetByUserByUnit(int UserId, int UnitId);
        Task<bool> ValidateUsernameAsync(string? username, int? id = null);
        Task<bool> ValidateUserActiveAsync(string? username, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ValidateUserRolesAsync(string username);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> UserLockedAsync(string UserName);
        Task<bool> IsFirstimeUserValidation(string UserName);
        
         
  
    }

}