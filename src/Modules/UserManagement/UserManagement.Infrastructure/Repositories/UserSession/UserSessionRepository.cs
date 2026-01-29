
using UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces.IUserSession;
using Infrastructure;
using Core.Application.Common.Interfaces;
using System.Data;
using Dapper;

namespace UserManagement.Infrastructure.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;   
        private readonly ITimeZoneService _timeZoneService;    
        private readonly IDbConnection _dbConnection;   

        public UserSessionRepository(ApplicationDbContext applicationDbContext, ITimeZoneService timeZoneService, IDbConnection dbConnection)
        {
            _applicationDbContext = applicationDbContext;    
            _timeZoneService = timeZoneService;     
            _dbConnection = dbConnection;            
        }       
        public async Task AddSessionAsync(UserSessions session)
        {
            _applicationDbContext.UserSession.Add(session);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<UserSessions> GetSessionByJwtIdAsync(string jwtId)
        {
            return await _applicationDbContext.UserSession.FirstOrDefaultAsync(s => s.JwtId == jwtId)  ?? new UserSessions();
        }

        public async Task UpdateSessionAsync(UserSessions session)
        {
            _applicationDbContext.UserSession.Update(session);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task DeactivateUserSessionsAsync(int userId)
        {
            
            var sessions = await _applicationDbContext.UserSession.Where(s => s.UserId == userId && s.IsActive==1).ToListAsync();
            foreach (var session in sessions)
            {
                session.IsActive = 0;
            }
            await _applicationDbContext.SaveChangesAsync();
        }
        public async Task<UserSessions> GetSessionByUserIdAsync(int userId)
        {
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
            return await _applicationDbContext.UserSession
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive == 1 && s.ExpiresAt > currentTime) ;
        }
        public async Task ExpireTokenAsync(string jwtId)
        {
            var session = await _applicationDbContext.UserSession.FirstOrDefaultAsync(s => s.JwtId == jwtId);
            if (session != null)
            {
                session.IsActive = 0;
                await _applicationDbContext.SaveChangesAsync();
            }
        }
        public async Task DeactivateExpiredSessionsAsync()
        {           
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
            var expiredSessions = await _applicationDbContext.UserSession
                .Where(s => s.ExpiresAt <= currentTime && s.IsActive==1)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = 0;
            }

            await _applicationDbContext.SaveChangesAsync();
        }
           public async Task<bool> ValidateUserSession(string username )
          {
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
              var query = @"SELECT COUNT(1) FROM [AppSecurity].[Users] U
            INNER JOIN [AppSecurity].[UserSessions] US ON US.UserId = U.UserId WHERE U.UserName = @username AND US.IsActive = 1 AND US.ExpiresAt > @currentTime";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { username = username, currentTime = currentTime });
                return count > 0;
          }
           public async Task<bool> DeactivateUserSessionsByUsername(string username)
           {
                var user = await _applicationDbContext.User
                                   .FirstOrDefaultAsync(u => u.UserName == username);
               var sessions = await _applicationDbContext.UserSession.Where(s => s.UserId == user.UserId && s.IsActive==1).ToListAsync();
               foreach (var session in sessions)
               {
                   session.IsActive = 0;
               }
              return await _applicationDbContext.SaveChangesAsync() > 0;
           }
    }
}