using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalFlag;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule
{
    public class JournalFlagEngineRepository : IJournalFlagEngineRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public JournalFlagEngineRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<ActiveThresholdRule>> GetActiveThresholdRulesAsync(CancellationToken ct)
        {
            return await _dbContext.JournalThresholdRule
                .Where(r => r.Active && r.IsDeleted == IsDelete.NotDeleted)
                .Select(r => new ActiveThresholdRule
                {
                    RuleTypeId = r.RuleTypeId,
                    RuleTypeCode = r.RuleType!.Code,
                    ThresholdValue = r.ThresholdValue
                })
                .ToListAsync(ct);
        }

        public async Task AddFlagsAsync(IEnumerable<JournalFlag> flags, CancellationToken ct)
        {
            await _dbContext.JournalFlag.AddRangeAsync(flags, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<JournalFlag>> GetUndigestedFlagsAsync(CancellationToken ct)
        {
            return await _dbContext.JournalFlag
                .Where(f => !f.DigestSent)
                .OrderBy(f => f.FlaggedAt)
                .ToListAsync(ct);
        }

        public async Task MarkDigestSentAsync(IEnumerable<int> flagIds, CancellationToken ct)
        {
            var ids = flagIds.ToList();
            if (ids.Count == 0)
                return;

            var rows = await _dbContext.JournalFlag.Where(f => ids.Contains(f.Id)).ToListAsync(ct);
            foreach (var r in rows)
                r.DigestSent = true;
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
