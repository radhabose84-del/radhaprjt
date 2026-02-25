using UserManagement.Application.Common.Interfaces.IUser;

namespace UserManagement.Infrastructure.Repositories.Users
{
    public class SuperAdminLoginPolicy : ILoginPolicy
    {
        public Task<string> CanAttemptLogin(string username, DateTime currentTime)
         {
             
             return Task.FromResult("Invalid username or password.");
         }
    }
}