using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ItemPriceMaster
{
    public class ItemPriceMasterCommandRepository : IItemPriceMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ItemPriceMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.ItemPriceMaster entity)
        {
            await _applicationDbContext.ItemPriceMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ItemPriceMaster entity)
        {
            var existingEntity = await _applicationDbContext.ItemPriceMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.ItemId = entity.ItemId;
            existingEntity.SalesSegmentId = entity.SalesSegmentId;
            existingEntity.PaymentTermsId = entity.PaymentTermsId;
            existingEntity.ExMillPrice = entity.ExMillPrice;
            existingEntity.CurrencyId = entity.CurrencyId;
            existingEntity.ValidFrom = entity.ValidFrom;
            existingEntity.ValidTo = entity.ValidTo;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.ItemPriceMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ItemPriceMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ItemPriceMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
