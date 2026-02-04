using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IUserRoleAllocation
{
    public interface IUserRoleAllocationQueryRepository
    {
    Task<List<UserManagement.Domain.Entities.UserRoleAllocation>> GetAllAsync();
    Task<UserManagement.Domain.Entities.UserRoleAllocation?> GetByIdAsync(int id);
    Task<List<UserManagement.Domain.Entities.UserRoleAllocation>> GetByUserIdAsync(int userId);
    }
}