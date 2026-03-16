using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.UsageType
{
    public class UsageTypeCommandRepository : IUsageTypeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UsageTypeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.UsageType entity)
        {
            await _dbContext.UsageType.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.UsageType entity)
        {
            var existing = await _dbContext.UsageType
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.UsageTypeName = entity.UsageTypeName;
            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;

            _dbContext.UsageType.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.UsageType
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.UsageType.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
