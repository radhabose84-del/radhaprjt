using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IProfile
{
    public interface IProfileQuery
    {
        Task<List<Unit>> GetUnit(int userId);
    }
}