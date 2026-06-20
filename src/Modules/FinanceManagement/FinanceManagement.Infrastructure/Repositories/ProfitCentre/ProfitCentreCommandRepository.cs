using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.ProfitCentre
{
    public class ProfitCentreCommandRepository : IProfitCentreCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ProfitCentreCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.ProfitCentre entity)
        {
            await _applicationDbContext.ProfitCentre.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ProfitCentre entity)
        {
            var existingEntity = await _applicationDbContext.ProfitCentre
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Code, LevelId, ParentProfitCentreId are immutable.
            existingEntity.ProfitCentreName = entity.ProfitCentreName;
            existingEntity.IsActive = entity.IsActive;
            existingEntity.IsRevenueLinked = entity.IsRevenueLinked;
            existingEntity.ResponsibleHeadId = entity.ResponsibleHeadId;

            _applicationDbContext.ProfitCentre.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ProfitCentre
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ProfitCentre.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
