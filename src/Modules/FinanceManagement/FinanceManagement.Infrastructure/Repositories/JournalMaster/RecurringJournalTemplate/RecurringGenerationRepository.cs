using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate
{
    public class RecurringGenerationRepository : IRecurringGenerationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RecurringGenerationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<RecurringJournalTemplateHeader>> GetDueTemplatesAsync(DateOnly periodDate, CancellationToken ct)
        {
            return await _dbContext.RecurringJournalTemplateHeader
                .Include(t => t.Lines)
                .Where(t => t.IsActive == Status.Active && t.IsDeleted == IsDelete.NotDeleted
                    && t.StartDate <= periodDate
                    && (t.EndDate == null || t.EndDate >= periodDate))
                .ToListAsync(ct);
        }

        public async Task<bool> GenerationExistsAsync(int companyId, int templateId, string period, CancellationToken ct)
        {
            return await _dbContext.RecurringGenerationLog
                .AnyAsync(g => g.CompanyId == companyId && g.TemplateId == templateId && g.Period == period, ct);
        }

        public async Task<int> CreateJournalWithLogAsync(JournalHeader header, RecurringGenerationLog log, CancellationToken ct)
        {
            // One transaction so the journal and its idempotency-log row commit together — a crash between
            // them can never leave an orphan draft that the next scheduled run would regenerate.
            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            await _dbContext.JournalHeader.AddAsync(header, ct);
            await _dbContext.SaveChangesAsync(ct);   // header.Id assigned

            log.GeneratedVoucherId = header.Id;
            await _dbContext.RecurringGenerationLog.AddAsync(log, ct);
            await _dbContext.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return header.Id;
        }

        public async Task MarkLogAutoPostedAsync(int logId, CancellationToken ct)
        {
            var log = await _dbContext.RecurringGenerationLog.FirstOrDefaultAsync(g => g.Id == logId, ct);
            if (log == null)
                return;

            log.AutoPosted = true;
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
