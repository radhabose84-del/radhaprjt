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
            JournalImportBatch batch, List<JournalHeader> drafts, CancellationToken ct)
        {
            // DbContext uses EnableRetryOnFailure, so a user-initiated transaction must run inside the
            // execution strategy (which retries the whole unit on a transient fault).
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

                await _dbContext.JournalImportBatch.AddAsync(batch, ct);
                await _dbContext.SaveChangesAsync(ct);

                foreach (var d in drafts)
                    d.ImportBatchId = batch.Id;

                await _dbContext.JournalHeader.AddRangeAsync(drafts, ct);
                await _dbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                return (batch.Id, drafts.Select(d => d.Id).ToList());
            });
        }
    }
}
