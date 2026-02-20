#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesChannel
{
    public class SalesChannelCommandRepository : ISalesChannelCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesChannelCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesChannel entity)
        {
            await _applicationDbContext.SalesChannel.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesChannel entity)
        {
            var existingEntity = await _applicationDbContext.SalesChannel
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.SalesChannelName = entity.SalesChannelName;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.SalesChannel.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesChannel
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesChannel.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
