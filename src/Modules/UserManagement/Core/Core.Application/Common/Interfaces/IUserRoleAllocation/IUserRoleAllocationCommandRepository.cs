using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IUserRoleAllocation
{
    public interface IUserRoleAllocationCommandRepository
    {
    Task CreateAsync(Core.Domain.Entities.UserRoleAllocation userRoleAllocation);
    Task AddRangeAsync(List<Core.Domain.Entities.UserRoleAllocation> userRoleAllocations);
    Task UpdateAsync(Core.Domain.Entities.UserRoleAllocation userRoleAllocation);
    Task DeleteAsync(int id);
    }
}