using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.VendorRatingGrade
{
    public class VendorRatingGradeCommandRepository : IVendorRatingGradeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VendorRatingGradeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorRatingGrade entity)
        {
            await _dbContext.VendorRatingGrades.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorRatingGrade entity)
        {
            var existing = await _dbContext.VendorRatingGrades
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null) return 0;

            existing.GradeName = entity.GradeName;
            existing.MinScore = entity.MinScore;
            existing.MaxScore = entity.MaxScore;
            existing.ActionDescription = entity.ActionDescription;
            existing.ActionTypeId = entity.ActionTypeId;
            existing.SortOrder = entity.SortOrder;
            existing.IsActive = entity.IsActive;

            _dbContext.VendorRatingGrades.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.VendorRatingGrades
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null) return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.VendorRatingGrades.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
