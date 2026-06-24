using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule
{
    public class JournalThresholdRuleCommandRepository : IJournalThresholdRuleCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public JournalThresholdRuleCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(FinanceManagement.Domain.Entities.JournalThresholdRule entity)
        {
            await _dbContext.JournalThresholdRule.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(FinanceManagement.Domain.Entities.JournalThresholdRule entity)
        {
            var existing = await _dbContext.JournalThresholdRule
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.RuleTypeId = entity.RuleTypeId;
            existing.ThresholdValue = entity.ThresholdValue;
            existing.Active = entity.Active;
            existing.EffectiveFrom = entity.EffectiveFrom;
            existing.IsActive = entity.IsActive;

            _dbContext.JournalThresholdRule.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.JournalThresholdRule
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.JournalThresholdRule.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
