using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DispatchAddressMapping
{
    public class DispatchAddressMappingCommandRepository : IDispatchAddressMappingCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DispatchAddressMappingCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.DispatchAddressMapping entity)
        {
            await _applicationDbContext.DispatchAddressMapping.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.DispatchAddressMapping entity)
        {
            var existingEntity = await _applicationDbContext.DispatchAddressMapping
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Only mutable fields — PartyId, DispatchAddressId, UsageTypeId are immutable
            existingEntity.IsDefault = entity.IsDefault;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.DispatchAddressMapping.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.DispatchAddressMapping
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.DispatchAddressMapping.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
