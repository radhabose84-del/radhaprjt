using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Repositories.UserRoleAllocation;
using Core.Application.Common.Interfaces.IUserRoleAllocation;

namespace UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationCommandRepository
{
    public class UserRoleAllocationCommandRepository :IUserRoleAllocationCommandRepository
    {
        
        private readonly ApplicationDbContext _applicationDbContext;
        IUserRoleAllocationQueryRepository _userRoleAllocationQueryRepository;

        public  UserRoleAllocationCommandRepository(ApplicationDbContext applicationDbContext, IUserRoleAllocationQueryRepository userRoleAllocationQueryRepository)
        {
            _applicationDbContext=applicationDbContext;
            _userRoleAllocationQueryRepository = userRoleAllocationQueryRepository;
        } 
        public async Task CreateAsync(Core.Domain.Entities.UserRoleAllocation userRoleAllocation)
        {
            await _applicationDbContext.UserRoleAllocations.AddAsync(userRoleAllocation);
            await _applicationDbContext.SaveChangesAsync();
        }

    public async Task AddRangeAsync(List<Core.Domain.Entities.UserRoleAllocation> userRoleAllocations)
        {
            await _applicationDbContext.UserRoleAllocations.AddRangeAsync(userRoleAllocations);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Core.Domain.Entities.UserRoleAllocation userRoleAllocation)
        {
            _applicationDbContext.UserRoleAllocations.Update(userRoleAllocation);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var allocation = await _userRoleAllocationQueryRepository.GetByIdAsync(id);
            if (allocation != null)
            {
                _applicationDbContext.UserRoleAllocations.Remove(allocation);
                await _applicationDbContext.SaveChangesAsync();
            }

        }
    }
}