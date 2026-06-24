using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate
{
    public class RecurringJournalTemplateCommandRepository : IRecurringJournalTemplateCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RecurringJournalTemplateCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader entity)
        {
            await _dbContext.RecurringJournalTemplateHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader entity)
        {
            var existing = await _dbContext.RecurringJournalTemplateHeader
                .Include(h => h.Lines)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.TemplateName = entity.TemplateName;
            existing.VoucherTypeId = entity.VoucherTypeId;
            existing.FrequencyId = entity.FrequencyId;
            existing.StartDate = entity.StartDate;
            existing.EndDate = entity.EndDate;
            existing.AutoPost = entity.AutoPost;
            existing.AmountAdjustmentRuleId = entity.AmountAdjustmentRuleId;
            existing.LowRisk = entity.LowRisk;
            existing.IsActive = entity.IsActive;
            _dbContext.RecurringJournalTemplateHeader.Update(existing);

            if (existing.Lines is { Count: > 0 })
                _dbContext.RecurringJournalTemplateDetail.RemoveRange(existing.Lines);

            foreach (var line in entity.Lines ?? new List<FinanceManagement.Domain.Entities.RecurringJournalTemplateDetail>())
            {
                line.Id = 0;
                line.TemplateId = existing.Id;
                await _dbContext.RecurringJournalTemplateDetail.AddAsync(line);
            }

            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.RecurringJournalTemplateHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.RecurringJournalTemplateHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
