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

        public async Task<int> CreateAsync(FinanceManagement.Domain.Entities.JournalHeader entity)
        {
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
            int journalId, int postedStatusId, string? financialYearName, int postedBy, DateTimeOffset postedAt, CancellationToken ct)
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            var header = await _dbContext.JournalHeader
                .Include(h => h.Details)
                .FirstOrDefaultAsync(h => h.Id == journalId && h.IsDeleted == IsDelete.NotDeleted, ct);

            // Already posted (number assigned) or missing → nothing to do.
            if (header == null || !string.IsNullOrEmpty(header.VoucherNo) || header.AccountingPeriodId == null)
                return null;

            var voucherType = await _dbContext.VoucherTypeMaster
                .FirstOrDefaultAsync(v => v.Id == header.VoucherTypeId, ct);
            if (voucherType == null)
                return null;

            // 1) Assign the next voucher number from the per-(type, FY) series (create the counter if absent).
            var series = await _dbContext.VoucherTypeNumberSeries
                .FirstOrDefaultAsync(s => s.VoucherTypeId == header.VoucherTypeId
                    && s.FinancialYearId == header.FinancialYearId
                    && s.IsDeleted == IsDelete.NotDeleted, ct);

            if (series == null)
            {
                series = new VoucherTypeNumberSeries
                {
                    VoucherTypeId = header.VoucherTypeId,
                    FinancialYearId = header.FinancialYearId,
                    LastUsedNumber = 0
                };
                await _dbContext.VoucherTypeNumberSeries.AddAsync(series, ct);
            }

            series.LastUsedNumber += 1;
            var padding = voucherType.NumberPadding > 0 ? voucherType.NumberPadding : 4;
            var voucherNo = $"{voucherType.VoucherTypeCode}/{financialYearName ?? "????"}/{series.LastUsedNumber.ToString().PadLeft(padding, '0')}";

            // 2) Stamp the header as posted.
            header.VoucherNo = voucherNo;
            header.StatusId = postedStatusId;
            header.PostingDate = DateOnly.FromDateTime(postedAt.DateTime);
            header.PostedBy = postedBy;
            header.PostedAt = postedAt;
            _dbContext.JournalHeader.Update(header);

            // 3) Upsert LedgerBalance per (account, period, cost-centre) in base currency.
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

            // One transaction: number + header + balances. RowVersion on LedgerBalance guards concurrent posts.
            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new PostJournalResultDto
            {
                JournalId = header.Id,
                VoucherNo = voucherNo,
                UpdatedBalances = updated
            };
        }
    }
}
