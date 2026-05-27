using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationCriteria
{
    public class VendorEvaluationCriteriaCommandRepository : IVendorEvaluationCriteriaCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VendorEvaluationCriteriaCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationCriteria entity)
        {
            await _dbContext.VendorEvaluationCriteria.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationCriteria entity)
        {
            var existing = await _dbContext.VendorEvaluationCriteria
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null) return 0;

            existing.CriteriaName = entity.CriteriaName;
            existing.Description = entity.Description;
            existing.WeightagePercent = entity.WeightagePercent;
            existing.ScoringMethodId = entity.ScoringMethodId;
            existing.MinimumScore = entity.MinimumScore;
            existing.RatingImpactId = entity.RatingImpactId;
            existing.SortOrder = entity.SortOrder;
            existing.IsActive = entity.IsActive;

            _dbContext.VendorEvaluationCriteria.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.VendorEvaluationCriteria
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null) return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.VendorEvaluationCriteria.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
