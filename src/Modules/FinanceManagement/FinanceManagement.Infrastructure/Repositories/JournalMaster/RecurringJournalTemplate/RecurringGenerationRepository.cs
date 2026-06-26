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

        public async Task<RecurringJournalTemplateHeader?> GetTemplateByIdAsync(int templateId, CancellationToken ct)
        {
            return await _dbContext.RecurringJournalTemplateHeader
                .Include(t => t.Lines)
                .FirstOrDefaultAsync(t => t.Id == templateId
                    && t.IsActive == Status.Active && t.IsDeleted == IsDelete.NotDeleted, ct);
        }

        public async Task<bool> GenerationExistsAsync(int companyId, int templateId, string period, CancellationToken ct)
        {
            // A generation only "counts" while its journal still exists — if the generated journal was soft-deleted,
            // the template can be regenerated for that period (the old log no longer blocks it).
            return await _dbContext.RecurringGenerationLog
                .Where(g => g.CompanyId == companyId && g.TemplateId == templateId && g.Period == period)
                .Where(g => g.GeneratedVoucherId == null
                            || !_dbContext.JournalHeader.Any(h => h.Id == g.GeneratedVoucherId && h.IsDeleted == IsDelete.Deleted))
                .AnyAsync(ct);
        }

        public async Task<int> CreateJournalWithLogAsync(JournalHeader header, RecurringGenerationLog log, string? financialYearName, CancellationToken ct)
        {
            // One transaction so the journal, its voucher number and its idempotency-log row commit together — a
            // crash between them can never leave an orphan draft (or a burnt voucher number) that the next run
            // would regenerate. DbContext uses EnableRetryOnFailure, so this runs inside the execution strategy.
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

                // Allocate the voucher number at create time so even non-posted (approved) recurring JVs carry one.
                if (!string.IsNullOrEmpty(financialYearName) && string.IsNullOrEmpty(header.VoucherNo))
                    header.VoucherNo = await GenerateVoucherNoAsync(
                        header.VoucherTypeId, header.FinancialYearId, financialYearName, header.CreatedBy, ct);

                await _dbContext.JournalHeader.AddAsync(header, ct);
                await _dbContext.SaveChangesAsync(ct);   // header.Id assigned

                // Upsert the generation log keyed on (Company, Template, Period). A prior log for this period may
                // already exist (e.g. its journal was soft-deleted and we are regenerating) — re-point it to the new
                // journal instead of inserting a duplicate (which would violate UX_RecurringGenerationLog_…Period).
                var existingLog = await _dbContext.RecurringGenerationLog.FirstOrDefaultAsync(
                    g => g.CompanyId == log.CompanyId && g.TemplateId == log.TemplateId && g.Period == log.Period, ct);

                if (existingLog != null)
                {
                    existingLog.GeneratedVoucherId = header.Id;
                    existingLog.GeneratedAt = log.GeneratedAt;
                    existingLog.AutoPosted = false;
                    _dbContext.RecurringGenerationLog.Update(existingLog);
                    log.Id = existingLog.Id;   // so MarkLogAutoPostedAsync targets the right row
                }
                else
                {
                    log.GeneratedVoucherId = header.Id;
                    await _dbContext.RecurringGenerationLog.AddAsync(log, ct);
                }
                await _dbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return header.Id;
            });
        }

        // Mirrors JournalCommandRepository.GenerateVoucherNoAsync — atomic MERGE on the per-(type, FY) number
        // series, formatted "{VoucherTypeCode}/{FY}/{padded number}". Must run inside the caller's transaction.
        private async Task<string?> GenerateVoucherNoAsync(
            int voucherTypeId, int financialYearId, string? financialYearName, int createdById, CancellationToken ct)
        {
            var voucherType = await _dbContext.VoucherTypeMaster.FirstOrDefaultAsync(v => v.Id == voucherTypeId, ct);
            if (voucherType == null)
                return null;

            var nextNumber = (await _dbContext.Database.SqlQueryRaw<int>(
                @"MERGE Finance.VoucherTypeNumberSeries WITH (HOLDLOCK) AS t
                  USING (SELECT {0} AS VoucherTypeId, {1} AS FinancialYearId) AS s
                      ON t.VoucherTypeId = s.VoucherTypeId AND t.FinancialYearId = s.FinancialYearId AND t.IsDeleted = 0
                  WHEN MATCHED THEN
                      UPDATE SET LastUsedNumber = t.LastUsedNumber + 1
                  WHEN NOT MATCHED THEN
                      INSERT (VoucherTypeId, FinancialYearId, LastUsedNumber, IsActive, IsDeleted, CreatedBy, CreatedDate)
                      VALUES (s.VoucherTypeId, s.FinancialYearId, 1, 1, 0, {2}, SYSDATETIMEOFFSET())
                  OUTPUT INSERTED.LastUsedNumber AS [Value];",
                voucherTypeId, financialYearId, createdById).ToListAsync(ct)).Single();

            var padding = voucherType.NumberPadding > 0 ? voucherType.NumberPadding : 4;
            return $"{voucherType.VoucherTypeCode}/{financialYearName ?? "????"}/{nextNumber.ToString().PadLeft(padding, '0')}";
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
