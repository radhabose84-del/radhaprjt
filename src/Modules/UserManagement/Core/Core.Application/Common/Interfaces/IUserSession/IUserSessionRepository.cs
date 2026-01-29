using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IUserSession
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