using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.CountGroup
{
    public class CountGroupCommandRepository : ICountGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CountGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CountGroup entity)
        {
            await _applicationDbContext.CountGroup.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CountGroup entity)
        {
            var existingEntity = await _applicationDbContext.CountGroup
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.CountGroupName = entity.CountGroupName;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.CountGroup.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.CountGroup
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.CountGroup.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
