using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule
{
    public class GapScanRepository : IGapScanRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GapScanRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<NumberSeriesInfo>> GetActiveSeriesAsync(CancellationToken ct)
        {
            return await _dbContext.VoucherTypeNumberSeries
                .Where(s => s.LastUsedNumber > 0 && s.IsDeleted == IsDelete.NotDeleted)
                .Select(s => new NumberSeriesInfo
                {
                    SeriesId = s.Id,
                    VoucherTypeId = s.VoucherTypeId,
                    FinancialYearId = s.FinancialYearId,
                    LastUsedNumber = s.LastUsedNumber
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<string>> GetUsedVoucherNumbersAsync(int voucherTypeId, int financialYearId, CancellationToken ct)
        {
            return await _dbContext.JournalHeader
                .Where(h => h.VoucherTypeId == voucherTypeId
                    && h.FinancialYearId == financialYearId
                    && h.VoucherNo != null
                    && h.IsDeleted == IsDelete.NotDeleted)
                .Select(h => h.VoucherNo!)
                .ToListAsync(ct);
        }

        public async Task AddScanLogAsync(SequenceGapScanLog log, CancellationToken ct)
        {
            await _dbContext.SequenceGapScanLog.AddAsync(log, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
