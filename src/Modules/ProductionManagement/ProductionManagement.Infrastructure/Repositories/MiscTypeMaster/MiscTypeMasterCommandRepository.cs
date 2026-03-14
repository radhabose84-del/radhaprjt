using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterCommandRepository : IMiscTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MiscTypeMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.MiscTypeMaster entity)
        {
            await _dbContext.MiscTypeMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.MiscTypeMaster entity)
        {
            var existing = await _dbContext.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;

            _dbContext.MiscTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.MiscTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
