using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.StoTypeMaster
{
    public class StoTypeMasterCommandRepository : IStoTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StoTypeMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.StoTypeMaster entity)
        {
            await _dbContext.StoTypeMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.StoTypeMaster entity)
        {
            var existing = await _dbContext.StoTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.StoTypeName = entity.StoTypeName;
            existing.Description = entity.Description;
            existing.PgiMovementTypeId = entity.PgiMovementTypeId;
            existing.GrMovementTypeId = entity.GrMovementTypeId;
            existing.IsActive = entity.IsActive;

            _dbContext.StoTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.StoTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.StoTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
