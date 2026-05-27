using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader
{
    public class VendorEvaluationHeaderCommandRepository : IVendorEvaluationHeaderCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VendorEvaluationHeaderCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity)
        {
            await _dbContext.VendorEvaluationHeaders.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity)
        {
            var existing = await _dbContext.VendorEvaluationHeaders
                .Include(h => h.VendorEvaluationDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null) return 0;

            // Update header fields (EvaluationCode is immutable)
            existing.VendorId = entity.VendorId;
            existing.EvaluationMonth = entity.EvaluationMonth;
            existing.EvaluationYear = entity.EvaluationYear;
            existing.EvaluationDate = entity.EvaluationDate;
            existing.TotalWeightedScore = entity.TotalWeightedScore;
            existing.GradeId = entity.GradeId;
            existing.StatusId = entity.StatusId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Replace details: remove old, add new
            if (existing.VendorEvaluationDetails != null)
            {
                _dbContext.VendorEvaluationDetails.RemoveRange(existing.VendorEvaluationDetails);
            }

            if (entity.VendorEvaluationDetails != null)
            {
                foreach (var detail in entity.VendorEvaluationDetails)
                {
                    detail.VendorEvaluationHeaderId = existing.Id;
                    await _dbContext.VendorEvaluationDetails.AddAsync(detail);
                }
            }

            _dbContext.VendorEvaluationHeaders.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.VendorEvaluationHeaders
                .Include(h => h.VendorEvaluationDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null) return false;

            // Soft delete header
            existing.IsDeleted = IsDelete.Deleted;

            // Cascade soft delete to details
            if (existing.VendorEvaluationDetails != null)
            {
                foreach (var detail in existing.VendorEvaluationDetails)
                {
                    detail.IsDeleted = IsDelete.Deleted;
                }
            }

            _dbContext.VendorEvaluationHeaders.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
