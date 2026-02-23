#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesGroup
{
    public class SalesGroupCommandRepository : ISalesGroupCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesGroupCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesGroup entity)
        {
            await _dbContext.SalesGroup.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesGroup entity)
        {
            var existing = await _dbContext.SalesGroup
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SalesGroupName = entity.SalesGroupName;
            existing.SalesOfficeId = entity.SalesOfficeId;
            existing.ResponsibleManager = entity.ResponsibleManager;
            existing.ProductCategoryId = entity.ProductCategoryId;
            existing.RegionTerritory = entity.RegionTerritory;
            existing.IsActive = entity.IsActive;

            _dbContext.SalesGroup.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.SalesGroup
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.SalesGroup.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
