using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IProfile
{
    public interface IProfileQuery
    {
        Task<List<Unit>> GetUnit(int userId);
    }
}