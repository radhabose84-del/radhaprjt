using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Maintenance;

namespace Contracts.Interfaces.External.IUser
{
    public interface IUnitGrpcClient
    {
        Task<List<UnitDto>> GetAllUnitAsync();
        Task<List<UnitDto>> GetUserUnitAsync(int userId);
    }
}