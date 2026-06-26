using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal
{
    public class JournalCommandRepository : IJournalCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public JournalCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(
            FinanceManagement.Domain.Entities.JournalHeader entity, string? financialYearName = null, int createdById = 0)
        {
            // When a fiscal-year name is supplied (manual entry, US-GL01-01), the voucher number is allocated
            // here at CREATE time. Number allocation (MERGE) + insert run in one transaction so a failed insert
            // can't burn a number. Other flows (reversal/recurring/import) call this without a name and get their
            // number later at post time.
            if (!string.IsNullOrEmpty(financialYearName) && string.IsNullOrEmpty(entity.VoucherNo))
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _dbContext.Database.BeginTransactionAsync();
                    entity.VoucherNo = await GenerateVoucherNoAsync(
                        entity.VoucherTypeId, entity.FinancialYearId, financialYearName, createdById, default);
                    await _dbContext.JournalHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();
                    return entity.Id;
                });
            }

            await _dbContext.JournalHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(FinanceManagement.Domain.Entities.JournalHeader entity)
        {
            var existing = await _dbContext.JournalHeader
                .Include(h => h.Details)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.VoucherTypeId = entity.VoucherTypeId;
            existing.VoucherDate = entity.VoucherDate;
            existing.Narration = entity.Narration;
            existing.AccountingPeriodId = entity.AccountingPeriodId;
            existing.FinancialYearId = entity.FinancialYearId;
            existing.TotalDr = entity.TotalDr;
            existing.TotalCr = entity.TotalCr;
            _dbContext.JournalHeader.Update(existing);

            // Draft lines have zero GL impact — replace them wholesale.
            if (existing.Details is { Count: > 0 })
                _dbContext.JournalDetail.RemoveRange(existing.Details);

            foreach (var line in entity.Details ?? new List<FinanceManagement.Domain.Entities.JournalDetail>())
            {
                line.Id = 0;
                line.JournalHeaderId = existing.Id;
                await _dbContext.JournalDetail.AddAsync(line);
            }

            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SetApprovalResultAsync(int id, int statusId, bool approved, string? actorName, DateTimeOffset at, CancellationToken ct)
        {
            // The journal is DRAFT (pending approval) here, so this status change is not blocked by the
            // posted-immutability trigger.
            var header = await _dbContext.JournalHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);
            if (header == null)
                return false;

            header.StatusId = statusId;
            if (approved) { header.ApprovedBy = actorName; header.ApprovedAt = at; }
            else { header.RejectedBy = actorName; header.RejectedAt = at; }

            _dbContext.JournalHeader.Update(header);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.JournalHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.JournalHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<PostJournalResultDto?> PostAsync(
            int journalId, int postedStatusId, string? financialYearName, string? postedByName, int postedById, DateTimeOffset postedAt, CancellationToken ct, DateOnly? postingDate = null)
        {
            // DbContext uses EnableRetryOnFailure, so a user-initiated transaction must run inside the
            // execution strategy (which retries the whole unit on a transient fault).
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            var header = await _dbContext.JournalHeader
                .Include(h => h.Details)
                .FirstOrDefaultAsync(h => h.Id == journalId && h.IsDeleted == IsDelete.NotDeleted, ct);

            // Already posted (posting date stamped) or missing → nothing to do. (VoucherNo may already be set
            // from create time, so the "already posted" guard is PostingDate, not VoucherNo.)
            if (header == null || header.PostingDate != null || header.AccountingPeriodId == null)
                return null;

            var result = await ApplyPostingAsync(header, postedStatusId, financialYearName, postedByName, postedById, postedAt, ct, postingDate);
            if (result == null)
                return null;

            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return result;
            });
        }

        public async Task<PostJournalResultDto?> ReverseAsync(
            FinanceManagement.Domain.Entities.JournalHeader reversal, int originalId, int postedStatusId,
            int reversedStatusId, string? financialYearName, string? postedByName, int postedById, DateTimeOffset postedAt, CancellationToken ct)
        {
            // Atomic reversal: create the mirror, post it (number + ledger), and flip the original to REVERSED
            // — all in ONE transaction, so a failure can never leave an orphaned posted reversal.
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

                await _dbContext.JournalHeader.AddAsync(reversal, ct);
                await _dbContext.SaveChangesAsync(ct);   // reversal.Id assigned

                var result = await ApplyPostingAsync(reversal, postedStatusId, financialYearName, postedByName, postedById, postedAt, ct);
                if (result == null)
                    return null;

                var original = await _dbContext.JournalHeader
                    .FirstOrDefaultAsync(h => h.Id == originalId && h.IsDeleted == IsDelete.NotDeleted, ct);
                if (original == null)
                    return null;

                // Flip the original to REVERSED (sanctioned POSTED -> REVERSED; trigger allows it). The original's
                // lines are kept intact for the audit trail; the mirror voucher contra-posts so the two net to zero.
                original.StatusId = reversedStatusId;
                _dbContext.JournalHeader.Update(original);
                await _dbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return result;
            });
        }

        // Allocate the next voucher number ATOMICALLY — a single MERGE…OUTPUT under HOLDLOCK increments the
        // per-(type, FY) counter (creating it on first use) and returns the new value, so concurrent allocators
        // serialize on this row and each gets a distinct sequential number. Returns null if the voucher type is missing.
        private async Task<string?> GenerateVoucherNoAsync(
            int voucherTypeId, int financialYearId, string? financialYearName, int createdById, CancellationToken ct)
        {
            var voucherType = await _dbContext.VoucherTypeMaster
                .FirstOrDefaultAsync(v => v.Id == voucherTypeId, ct);
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

        // Shared posting core: stamp the header Posted (number already assigned) and upsert
        // LedgerBalance. Assumes the header is tracked and a transaction is already open; the caller saves
        // and commits. Returns null only if the voucher type is missing.
        private async Task<PostJournalResultDto?> ApplyPostingAsync(
            FinanceManagement.Domain.Entities.JournalHeader header, int postedStatusId, string? financialYearName,
            string? postedByName, int postedById, DateTimeOffset postedAt, CancellationToken ct, DateOnly? postingDate = null)
        {
            if (header.AccountingPeriodId == null)
                return null;

            // Manual vouchers already carry a number from create time; only allocate one here if it is still
            // empty (reversal / recurring / import flows that create without a number).
            if (string.IsNullOrEmpty(header.VoucherNo))
            {
                header.VoucherNo = await GenerateVoucherNoAsync(
                    header.VoucherTypeId, header.FinancialYearId, financialYearName, postedById, ct);
                if (header.VoucherNo == null)
                    return null;   // voucher type missing
            }

            var voucherNo = header.VoucherNo;
            header.StatusId = postedStatusId;
            header.IsPosted = true;
            header.PostingDate = postingDate ?? DateOnly.FromDateTime(postedAt.DateTime);
            header.PostedBy = postedByName;
            header.PostedAt = postedAt;
            _dbContext.JournalHeader.Update(header);

            var periodId = header.AccountingPeriodId.Value;
            var updated = new List<PostedBalanceDto>();

            var buckets = (header.Details ?? new List<JournalDetail>())
                .GroupBy(l => new { l.GlAccountId, l.CostCentreId })
                .Select(g => new
                {
                    g.Key.GlAccountId,
                    g.Key.CostCentreId,
                    Dr = g.Sum(x => x.BaseDrAmount ?? x.DrAmount),
                    Cr = g.Sum(x => x.BaseCrAmount ?? x.CrAmount)
                });

            foreach (var b in buckets)
            {
                var ccId = b.CostCentreId;
                var existing = ccId.HasValue
                    ? await _dbContext.LedgerBalance.FirstOrDefaultAsync(x =>
                        x.CompanyId == header.CompanyId && x.GlAccountId == b.GlAccountId &&
                        x.AccountingPeriodId == periodId && x.CostCentreId == ccId.Value, ct)
                    : await _dbContext.LedgerBalance.FirstOrDefaultAsync(x =>
                        x.CompanyId == header.CompanyId && x.GlAccountId == b.GlAccountId &&
                        x.AccountingPeriodId == periodId && x.CostCentreId == null, ct);

                if (existing == null)
                {
                    existing = new LedgerBalance
                    {
                        CompanyId = header.CompanyId,
                        GlAccountId = b.GlAccountId,
                        AccountingPeriodId = periodId,
                        CostCentreId = ccId,
                        FinancialYearId = header.FinancialYearId,
                        DrTotal = 0,
                        CrTotal = 0,
                        Balance = 0
                    };
                    await _dbContext.LedgerBalance.AddAsync(existing, ct);
                }

                existing.DrTotal += b.Dr;
                existing.CrTotal += b.Cr;
                existing.Balance = existing.DrTotal - existing.CrTotal;

                updated.Add(new PostedBalanceDto
                {
                    GlAccountId = b.GlAccountId,
                    CostCentreId = ccId,
                    Balance = existing.Balance
                });
            }

            return new PostJournalResultDto
            {
                JournalId = header.Id,
                VoucherNo = voucherNo,
                UpdatedBalances = updated
            };
        }
    }
}
