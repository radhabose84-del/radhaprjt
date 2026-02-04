using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IUnit
{
    public interface IUnitGrpcQuery
    {
        Task<List<Unit>> GetUserUnitsAsync(int UserId);
    }
}