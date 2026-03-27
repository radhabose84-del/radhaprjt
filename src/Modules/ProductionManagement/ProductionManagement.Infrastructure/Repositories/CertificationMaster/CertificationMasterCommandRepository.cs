using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.CertificationMaster
{
    public class CertificationMasterCommandRepository : ICertificationMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CertificationMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CertificationMaster entity)
        {
            await _applicationDbContext.CertificationMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CertificationMaster entity)
        {
            var existingEntity = await _applicationDbContext.CertificationMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.CertificationName = entity.CertificationName;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.CertificationMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.CertificationMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.CertificationMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
