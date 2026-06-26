using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalImport
{
    public class JournalImportCommandRepository : IJournalImportCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public JournalImportCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveFailedBatchAsync(
            JournalImportBatch batch, IEnumerable<JournalImportError> errors, CancellationToken ct)
        {
            await _dbContext.JournalImportBatch.AddAsync(batch, ct);
            await _dbContext.SaveChangesAsync(ct);

            foreach (var e in errors)
                e.ImportBatchId = batch.Id;
            await _dbContext.JournalImportError.AddRangeAsync(errors, ct);
            await _dbContext.SaveChangesAsync(ct);

            return batch.Id;
        }

        public async Task<(int BatchId, List<int> JournalIds)> CommitAsync(
            JournalImportBatch batch, List<JournalHeader> drafts, IReadOnlyDictionary<int, string> financialYearNames, CancellationToken ct)
        {
            // DbContext uses EnableRetryOnFailure, so a user-initiated transaction must run inside the
            // execution strategy (which retries the whole unit on a transient fault).
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

                await _dbContext.JournalImportBatch.AddAsync(batch, ct);
                await _dbContext.SaveChangesAsync(ct);

                // Allocate the voucher number at import time (like manual create) — in this transaction, so a
                // failed insert can't burn a number. Drafts keep the number through to posting.
                foreach (var d in drafts)
                {
                    d.ImportBatchId = batch.Id;
                    if (string.IsNullOrEmpty(d.VoucherNo)
                        && financialYearNames.TryGetValue(d.FinancialYearId, out var fyName))
                    {
                        d.VoucherNo = await GenerateVoucherNoAsync(d.VoucherTypeId, d.FinancialYearId, fyName, d.CreatedBy, ct);
                    }
                }

                await _dbContext.JournalHeader.AddRangeAsync(drafts, ct);
                await _dbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                return (batch.Id, drafts.Select(d => d.Id).ToList());
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
    }
}
