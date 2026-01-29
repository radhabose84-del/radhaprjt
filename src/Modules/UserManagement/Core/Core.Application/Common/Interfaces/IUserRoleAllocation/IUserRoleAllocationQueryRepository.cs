using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IUserRoleAllocation
{
    public interface IUserRoleAllocationQueryRepository
    {
    Task<List<Core.Domain.Entities.UserRoleAllocation>> GetAllAsync();
    Task<Core.Domain.Entities.UserRoleAllocation?> GetByIdAsync(int id);
    Task<List<Core.Domain.Entities.UserRoleAllocation>> GetByUserIdAsync(int userId);
    }
}