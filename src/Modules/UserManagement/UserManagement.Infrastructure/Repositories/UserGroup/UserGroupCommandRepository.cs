using Core.Application.Common.Interfaces.IUserGroup;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.UserGroup
{
    public class UserGroupCommandRepository : IUserGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        
        public UserGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<Core.Domain.Entities.UserGroup> CreateAsync(Core.Domain.Entities.UserGroup userGroups)
        {
            await _applicationDbContext.UserGroup.AddAsync(userGroups);
            await _applicationDbContext.SaveChangesAsync();
            return userGroups;
        }

        public async Task<int> DeleteAsync(int id, Core.Domain.Entities.UserGroup userGroups)
        {
             var userGroupToDelete = await _applicationDbContext.UserGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (userGroupToDelete != null)
            {
                userGroupToDelete.IsDeleted = userGroups.IsDeleted;                 
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; 
        }
        public async Task<int> UpdateAsync(int id, Core.Domain.Entities.UserGroup userGroups)
        {
            var existingUserGroup = await _applicationDbContext.UserGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUserGroup != null)
            {
                existingUserGroup.GroupName = userGroups.GroupName;
                existingUserGroup.GroupCode = userGroups.GroupCode;                
                existingUserGroup.IsActive = userGroups.IsActive;
                _applicationDbContext.UserGroup.Update(existingUserGroup);
                return await _applicationDbContext.SaveChangesAsync();
            }
           return 0;
        }

        public async Task<Core.Domain.Entities.UserGroup> GetUserGroupByCodeAsync(string groupName,string groupCode)
        {
               return await _applicationDbContext.UserGroup
            .FirstOrDefaultAsync(c => c.GroupName == groupName && c.GroupCode == groupCode) ?? new Core.Domain.Entities.UserGroup();
        }

    }
}