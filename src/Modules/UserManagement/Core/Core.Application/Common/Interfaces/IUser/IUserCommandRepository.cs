using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IUser
{
    public interface IUserCommandRepository
    {
        Task<User> CreateAsync(User user);
        Task<int> UpdateAsync(int userId, User user);
        Task<bool> DeleteAsync(int userId, User user);
        Task<int> SetAdminPassword(int userId, User user);
        Task<bool> UnlockUser(string username);
        Task<bool> lockUser(string username);
        Task<bool> RemoveVerficationCode(string username);

        Task<int> GetMiscmasterByIdAsync(string miscTypeCode, string code);
    }

}