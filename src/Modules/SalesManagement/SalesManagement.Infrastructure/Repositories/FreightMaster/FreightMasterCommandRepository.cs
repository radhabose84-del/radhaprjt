using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.FreightMaster
{
    public class FreightMasterCommandRepository : IFreightMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FreightMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.FreightMaster entity)
        {
            _dbContext.FreightMaster.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.FreightMaster entity)
        {
            var existing = await _dbContext.FreightMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.FreightModeId = entity.FreightModeId;
            existing.RateMethodId = entity.RateMethodId;
            existing.Rate = entity.Rate;
            existing.IsActive = entity.IsActive;

            _dbContext.FreightMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.FreightMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.FreightMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
