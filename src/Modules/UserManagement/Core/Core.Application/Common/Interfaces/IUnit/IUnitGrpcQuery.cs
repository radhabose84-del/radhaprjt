using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IUnit
{
    public interface IUnitGrpcQuery
    {
        Task<List<Unit>> GetUserUnitsAsync(int UserId);
    }
}