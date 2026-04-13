using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Infrastructure.Repositories.ItemSpecificationValue
{
    public class ItemSpecificationValueCommandRepository : IItemSpecificationValueCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ItemSpecificationValueCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(DomainEntities.ItemSpecificationValue entity)
        {
            await _dbContext.ItemSpecificationValue.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(DomainEntities.ItemSpecificationValue entity)
        {
            var existing = await _dbContext.ItemSpecificationValue
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SpecificationMasterId = entity.SpecificationMasterId;
            existing.SpecificationValue = entity.SpecificationValue;
            existing.IsActive = entity.IsActive;

            _dbContext.ItemSpecificationValue.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.ItemSpecificationValue
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, cancellationToken);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ItemSpecificationValue.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
