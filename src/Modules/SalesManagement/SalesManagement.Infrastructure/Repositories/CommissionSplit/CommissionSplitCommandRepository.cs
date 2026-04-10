using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.CommissionSplit
{
    public class CommissionSplitCommandRepository : ICommissionSplitCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CommissionSplitCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CommissionSplit entity)
        {
            // Auto-generate SplitCode
            var lastId = await _dbContext.CommissionSplit
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            entity.SplitCode = $"CSP{(lastId + 1):D5}";

            _dbContext.CommissionSplit.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CommissionSplit entity)
        {
            var existing = await _dbContext.CommissionSplit
                .Include(d => d.CommissionSplitDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update header fields (SplitCode is immutable — not updated)
            existing.SplitName = entity.SplitName;
            existing.IsActive = entity.IsActive;

            // Replace strategy: remove old children, add new ones
            if (existing.CommissionSplitDetails != null)
                _dbContext.CommissionSplitDetail.RemoveRange(existing.CommissionSplitDetails);

            // Add new children
            if (entity.CommissionSplitDetails != null && entity.CommissionSplitDetails.Count > 0)
            {
                foreach (var detail in entity.CommissionSplitDetails)
                {
                    detail.CommissionSplitId = existing.Id;
                    _dbContext.CommissionSplitDetail.Add(detail);
                }
            }

            _dbContext.CommissionSplit.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.CommissionSplit
                .Include(d => d.CommissionSplitDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            // Soft-delete child records
            if (existing.CommissionSplitDetails != null)
            {
                foreach (var detail in existing.CommissionSplitDetails)
                    detail.IsDeleted = IsDelete.Deleted;
            }

            _dbContext.CommissionSplit.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
