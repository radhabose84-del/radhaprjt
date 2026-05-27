using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.DeliveryScoreRule
{
    public class DeliveryScoreRuleCommandRepository : IDeliveryScoreRuleCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DeliveryScoreRuleCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.VendorEvaluation.DeliveryScoreRule entity)
        {
            await _dbContext.DeliveryScoreRules.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.DeliveryScoreRule entity)
        {
            var existing = await _dbContext.DeliveryScoreRules
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null) return 0;

            existing.Description = entity.Description;
            existing.DelayDaysFrom = entity.DelayDaysFrom;
            existing.DelayDaysTo = entity.DelayDaysTo;
            existing.Score = entity.Score;
            existing.SortOrder = entity.SortOrder;
            existing.IsActive = entity.IsActive;

            _dbContext.DeliveryScoreRules.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.DeliveryScoreRules
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null) return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.DeliveryScoreRules.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
