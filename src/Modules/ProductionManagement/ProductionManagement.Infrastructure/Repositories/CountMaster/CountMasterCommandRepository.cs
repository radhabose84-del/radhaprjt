using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.CountMaster
{
    public class CountMasterCommandRepository : ICountMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CountMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CountMaster entity)
        {
            await _applicationDbContext.CountMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CountMaster entity)
        {
            var existingEntity = await _applicationDbContext.CountMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.CountValue = entity.CountValue;
            existingEntity.ShortName = entity.ShortName;
            existingEntity.CountCategoryId = entity.CountCategoryId;
            existingEntity.CountTypeId = entity.CountTypeId;
            existingEntity.CountDescription = entity.CountDescription;
            existingEntity.UOMId = entity.UOMId;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.CountMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.CountMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.CountMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
