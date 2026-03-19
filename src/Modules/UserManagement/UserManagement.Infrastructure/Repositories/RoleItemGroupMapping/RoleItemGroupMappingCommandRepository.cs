using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.RoleItemGroupMapping
{
    public class RoleItemGroupMappingCommandRepository : IRoleItemGroupMappingCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleItemGroupMappingCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Domain.Entities.RoleItemGroupMapping> CreateAsync(
            Domain.Entities.RoleItemGroupMapping entity)
        {
            await _dbContext.RoleItemGroupMapping.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<int> UpdateAsync(int id, Domain.Entities.RoleItemGroupMapping entity)
        {
            var existing = await _dbContext.RoleItemGroupMapping
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing != null)
            {
                existing.RoleId = entity.RoleId;
                existing.ItemGroupId = entity.ItemGroupId;
                existing.IsActive = entity.IsActive;
                _dbContext.RoleItemGroupMapping.Update(existing);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<int> DeleteAsync(int id, Domain.Entities.RoleItemGroupMapping entity)
        {
            var existing = await _dbContext.RoleItemGroupMapping
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing != null)
            {
                existing.IsDeleted = entity.IsDeleted;
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<bool> CompositeKeyExistsAsync(int roleId, int itemGroupId, int? excludeId = null)
        {
            return await _dbContext.RoleItemGroupMapping
                .AnyAsync(x => x.RoleId == roleId
                    && x.ItemGroupId == itemGroupId
                    && x.IsDeleted == Enums.IsDelete.NotDeleted
                    && (!excludeId.HasValue || x.Id != excludeId.Value));
        }
    }
}
