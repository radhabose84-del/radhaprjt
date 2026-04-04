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

        public async Task<int> UpdateAsync(SalesReturnHeader entity, List<SalesReturnDetail> details)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var resultId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.SalesReturnHeader
                        .Include(h => h.SalesReturnDetails)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                    {
                        resultId = 0;
                        return;
                    }

                    existing.WarehouseId = entity.WarehouseId;
                    existing.BinId = entity.BinId;
                    existing.Remarks = entity.Remarks;
                    existing.IsActive = entity.IsActive;

                    // Remove existing details and add new ones
                    if (existing.SalesReturnDetails != null && existing.SalesReturnDetails.Count > 0)
                    {
                        _dbContext.SalesReturnDetail.RemoveRange(existing.SalesReturnDetails);
                    }

                    if (details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.SalesReturnHeaderId = existing.Id;
                            detail.IsActive = Status.Active;
                            detail.IsDeleted = IsDelete.NotDeleted;
                            await _dbContext.SalesReturnDetail.AddAsync(detail);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    resultId = existing.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return resultId;
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
    }
}
