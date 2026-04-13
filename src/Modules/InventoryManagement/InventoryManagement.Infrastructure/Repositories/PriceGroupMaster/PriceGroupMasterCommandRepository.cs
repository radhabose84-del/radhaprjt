using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.PriceGroupMaster
{
    public class PriceGroupMasterCommandRepository : IPriceGroupMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PriceGroupMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(InventoryManagement.Domain.Entities.PriceGroupMaster entity)
        {
            await _dbContext.PriceGroupMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(InventoryManagement.Domain.Entities.PriceGroupMaster entity)
        {
            var existing = await _dbContext.PriceGroupMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // PriceGroupCode is immutable — never copied here
            existing.PriceGroupName = entity.PriceGroupName;
            existing.Description = entity.Description;
            existing.EffectiveFrom = entity.EffectiveFrom;
            existing.EffectiveTo = entity.EffectiveTo;
            existing.IsActive = entity.IsActive;

            _dbContext.PriceGroupMaster.Update(existing);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.PriceGroupMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, cancellationToken);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.PriceGroupMaster.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
