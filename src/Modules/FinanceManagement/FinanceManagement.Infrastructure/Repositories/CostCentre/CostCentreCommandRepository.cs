using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.CostCentre
{
    public class CostCentreCommandRepository : ICostCentreCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CostCentreCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CostCentre entity)
        {
            await _applicationDbContext.CostCentre.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CostCentre entity)
        {
            var existingEntity = await _applicationDbContext.CostCentre
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Code, CentreLevelId, ParentCostCentreId, UnitId (Plant) are immutable — only name + status change.
            existingEntity.CostCentreName = entity.CostCentreName;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.CostCentre.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.CostCentre
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.CostCentre.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
