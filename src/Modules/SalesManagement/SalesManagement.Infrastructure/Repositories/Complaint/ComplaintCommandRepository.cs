using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.Complaint
{
    public class ComplaintCommandRepository : IComplaintCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ComplaintCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(ComplaintHeader entity, int typeId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.ComplaintHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    // Increment DocNo in Finance.DocumentSequence (same EF Core connection = atomic)
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

        public async Task<int> UpdateAsync(ComplaintHeader entity, List<ComplaintDetail> details)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var resultId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.ComplaintHeader
                        .Include(h => h.ComplaintDetails!)
                            .ThenInclude(d => d.ComplaintDetailNatures!)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                    {
                        resultId = 0;
                        return;
                    }

                    // Update header fields
                    existing.ComplaintDate = entity.ComplaintDate;
                    existing.CustomerId = entity.CustomerId;
                    existing.CustomerAddress = entity.CustomerAddress;
                    existing.CustomerPIN = entity.CustomerPIN;
                    existing.CustomerMobile = entity.CustomerMobile;
                    existing.CustomerEmail = entity.CustomerEmail;
                    existing.CustomerPAN = entity.CustomerPAN;
                    existing.CustomerGSTNo = entity.CustomerGSTNo;
                    existing.CreditLimit = entity.CreditLimit;
                    existing.TotalOS = entity.TotalOS;
                    existing.Outstanding = entity.Outstanding;
                    existing.BalanceCredit = entity.BalanceCredit;
                    existing.Delay = entity.Delay;
                    existing.Ledger = entity.Ledger;
                    existing.Remarks = entity.Remarks;
                    existing.IsActive = entity.IsActive;

                    // Remove existing details and natures
                    if (existing.ComplaintDetails != null && existing.ComplaintDetails.Count > 0)
                    {
                        foreach (var existingDetail in existing.ComplaintDetails)
                        {
                            if (existingDetail.ComplaintDetailNatures != null)
                            {
                                _dbContext.ComplaintDetailNature.RemoveRange(existingDetail.ComplaintDetailNatures);
                            }
                        }
                        _dbContext.ComplaintDetail.RemoveRange(existing.ComplaintDetails);
                    }

                    // Add new details with natures
                    if (details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.ComplaintHeaderId = existing.Id;
                            detail.IsActive = Status.Active;
                            detail.IsDeleted = IsDelete.NotDeleted;
                            await _dbContext.ComplaintDetail.AddAsync(detail);
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
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ComplaintHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return;

            // Resolve the status Id from MiscMaster
            var statusEntity = await _dbContext.MiscMaster
                .Include(m => m.MiscTypeMaster)
                .FirstOrDefaultAsync(m =>
                    m.MiscTypeMaster != null &&
                    m.MiscTypeMaster.MiscTypeCode == "ApprovalStatus" &&
                    m.Code == status &&
                    m.IsDeleted == IsDelete.NotDeleted, ct);

            if (statusEntity != null)
            {
                existing.StatusId = statusEntity.Id;
                await _dbContext.SaveChangesAsync(ct);
            }
        }
    }
}
