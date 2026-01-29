using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IUser
{
    public interface ILoginPolicy
    {
        Task<string> CanAttemptLogin(string username, DateTime currentTime);
    }
}