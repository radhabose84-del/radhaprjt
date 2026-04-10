using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Infrastructure.Repositories.ItemSpecificationMaster
{
    public class ItemSpecificationMasterCommandRepository : IItemSpecificationMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ItemSpecificationMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(DomainEntities.ItemSpecificationMaster entity)
        {
            await _dbContext.ItemSpecificationMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(DomainEntities.ItemSpecificationMaster entity)
        {
            var existing = await _dbContext.ItemSpecificationMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SpecificationName = entity.SpecificationName;
            existing.Order = entity.Order;
            existing.IsActive = entity.IsActive;

            _dbContext.ItemSpecificationMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.ItemSpecificationMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, cancellationToken);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ItemSpecificationMaster.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
