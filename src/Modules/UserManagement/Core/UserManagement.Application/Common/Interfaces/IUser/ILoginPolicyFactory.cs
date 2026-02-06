using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IUser
{
    public interface ILoginPolicyFactory
    {
        Task<ILoginPolicy> GetPolicyAsync(User user);
    }
}