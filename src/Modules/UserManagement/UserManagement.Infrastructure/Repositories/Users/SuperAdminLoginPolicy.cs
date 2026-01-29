using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUser;

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