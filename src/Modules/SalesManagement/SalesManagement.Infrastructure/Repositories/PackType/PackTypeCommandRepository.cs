using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.PackType
{
    public class PackTypeCommandRepository : IPackTypeCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public PackTypeCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.PackType entity)
        {
            await _applicationDbContext.PackType.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.PackType entity)
        {
            var existingEntity = await _applicationDbContext.PackType
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.PackTypeName = entity.PackTypeName;
            existingEntity.NetWeight = entity.NetWeight;
            existingEntity.TareWeight = entity.TareWeight;
            existingEntity.GrossWeight = entity.GrossWeight;
            existingEntity.ConesPerBag = entity.ConesPerBag;
            existingEntity.ProductionAllowed = entity.ProductionAllowed;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.PackType.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.PackType
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.PackType.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
