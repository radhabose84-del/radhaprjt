using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.YarnTwistMaster
{
    public class YarnTwistMasterCommandRepository : IYarnTwistMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public YarnTwistMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.YarnTwistMaster entity)
        {
            await _applicationDbContext.YarnTwistMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.YarnTwistMaster entity)
        {
            var existingEntity = await _applicationDbContext.YarnTwistMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.TwistName = entity.TwistName;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.YarnTwistMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.YarnTwistMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.YarnTwistMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
