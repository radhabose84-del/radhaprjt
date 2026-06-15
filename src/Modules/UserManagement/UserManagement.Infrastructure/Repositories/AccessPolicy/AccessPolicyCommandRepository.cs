using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Infrastructure.Data;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.AccessPolicy
{
    public class AccessPolicyCommandRepository : IAccessPolicyCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccessPolicyCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.AccessPolicy entity)
        {
            await _dbContext.AccessPolicies.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.AccessPolicy entity)
        {
            var existing = await _dbContext.AccessPolicies
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.PolicyName = entity.PolicyName;
            existing.EntityName = entity.EntityName;
            existing.FieldName  = entity.FieldName;
            existing.IsActive   = entity.IsActive;

            _dbContext.AccessPolicies.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.AccessPolicies
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.AccessPolicies.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> AssignRoleValueAsync(Domain.Entities.RoleAccessPolicy entity)
        {
            await _dbContext.RoleAccessPolicies.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> RemoveRoleValueAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.RoleAccessPolicies
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                return false;

            _dbContext.RoleAccessPolicies.Remove(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
