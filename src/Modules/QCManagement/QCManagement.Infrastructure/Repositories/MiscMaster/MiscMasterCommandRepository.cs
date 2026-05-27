using Microsoft.EntityFrameworkCore;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Infrastructure.Data;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterCommandRepository : IMiscMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public MiscMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.MiscMaster entity)
        {
            await _applicationDbContext.MiscMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.MiscMaster entity)
        {
            var existingEntity = await _applicationDbContext.MiscMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.Description = entity.Description;
            existingEntity.SortOrder = entity.SortOrder;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.MiscMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.MiscMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.MiscMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetMaxSortOrderAsync(int miscTypeId)
        {
            var maxOrder = await _applicationDbContext.MiscMaster
                .Where(x => x.MiscTypeId == miscTypeId && x.IsDeleted == IsDelete.NotDeleted)
                .MaxAsync(x => (int?)x.SortOrder);

            return maxOrder ?? 0;
        }
    }
}
