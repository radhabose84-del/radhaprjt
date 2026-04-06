using Microsoft.EntityFrameworkCore;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Infrastructure.Data;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.Infrastructure.Repositories.FreightMaster
{
    public class FreightMasterCommandRepository : IFreightMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public FreightMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.FreightMaster entity)
        {
            await _applicationDbContext.FreightMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.FreightMaster entity)
        {
            var existingEntity = await _applicationDbContext.FreightMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.FreightModeId = entity.FreightModeId;
            existingEntity.RateMethodId = entity.RateMethodId;
            existingEntity.Rate = entity.Rate;
            existingEntity.ModuleId = entity.ModuleId;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.FreightMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.FreightMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.FreightMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
