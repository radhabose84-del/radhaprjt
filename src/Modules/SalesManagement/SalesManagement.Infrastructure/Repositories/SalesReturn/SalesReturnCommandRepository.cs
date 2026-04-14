using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesReturn
{
    public class SalesReturnCommandRepository : ISalesReturnCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesReturnCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(SalesReturnHeader entity, int typeId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.SalesReturnHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    // Increment DocNo in Finance.DocumentSequence
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TransactionTypeId = {0} AND IsDeleted = 0",
                        typeId);

                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.SalesReturnHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.SalesReturnHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task InsertStockLedgerEntriesAsync(List<Domain.Entities.StockLedger> entries)
        {
            await _dbContext.StockLedger.AddRangeAsync(entries);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateComplaintResolutionReturnStatusAsync(int complaintHeaderId, int returnStatusId, decimal returnQuantity, int? closureStatusId = null, int? closedBy = null)
        {
            var resolution = await _dbContext.ComplaintResolution
                .FirstOrDefaultAsync(x => x.ComplaintHeaderId == complaintHeaderId && x.IsDeleted == IsDelete.NotDeleted);

            if (resolution == null) return;

            resolution.ReturnStatusId = returnStatusId;
            resolution.ReturnQuantity = returnQuantity;

            if (closureStatusId.HasValue)
            {
                resolution.ClosureStatusId = closureStatusId.Value;
                resolution.ClosedBy = closedBy;
                resolution.ClosedDate = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
