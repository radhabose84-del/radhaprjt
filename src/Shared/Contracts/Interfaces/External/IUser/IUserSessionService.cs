using System;
using System.Threading.Tasks;
using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IUserSessionService
    {
        Task<UserSessionDto?> GetSessionByJwtIdAsync(string jwtId, string token);
        Task<bool> UpdateSessionAsync(string jwtId, DateTimeOffset lastActivity, string token);
    }
}