using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IUserSession
{
    public interface IUserSessionRepository
    { 
        Task AddSessionAsync(UserSessions session);
        Task<UserSessions> GetSessionByJwtIdAsync(string jwtId);
        Task UpdateSessionAsync(UserSessions session);
        Task DeactivateUserSessionsAsync(int userId);
        Task<UserSessions> GetSessionByUserIdAsync(int userId);    
        Task DeactivateExpiredSessionsAsync();
        Task<bool> ValidateUserSession(string username  );   
        Task<bool> DeactivateUserSessionsByUsername(string username);         
    }
}