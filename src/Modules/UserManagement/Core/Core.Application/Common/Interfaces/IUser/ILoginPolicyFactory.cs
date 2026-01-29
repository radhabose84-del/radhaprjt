using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IUser
{
    public interface ILoginPolicyFactory
    {
        Task<ILoginPolicy> GetPolicyAsync(User user);
    }
}