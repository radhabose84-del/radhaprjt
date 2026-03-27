using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.QualityMaster
{
    public class QualityMasterCommandRepository : IQualityMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public QualityMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.QualityMaster entity)
        {
            await _applicationDbContext.QualityMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.QualityMaster entity)
        {
            var existingEntity = await _applicationDbContext.QualityMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.QualityName = entity.QualityName;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.QualityMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.QualityMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.QualityMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
