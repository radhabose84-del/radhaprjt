using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces
{
    public interface IChangePassword
    {
        Task<bool> FirstTimeUserChangePassword(int userId,PasswordLog passwordLog);
        Task<string> PasswordEncode(string password);
        Task<bool> ChangePassword(int userId,string password,PasswordLog passwordLog);
        Task<string> GenerateVerificationCode(int length);
        Task<string> ResetUserPassword(int userId,PasswordLog passwordLog);
        Task<bool> PasswordLog(PasswordLog passwordLog);
        Task<bool> ValidatePassword(int userId,string password);
        Task<bool> ValidateFirstTimeUser(int userId);
        Task<string?> GetUserPasswordHashAsync(int userId);
        Task<bool> ValidatePasswordbyUserName(string username,string password);


        
    }
}