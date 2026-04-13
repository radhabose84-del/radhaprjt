using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.RawMaterialType
{
    public class RawMaterialTypeCommandRepository : IRawMaterialTypeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RawMaterialTypeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.RawMaterialType entity)
        {
            await _dbContext.RawMaterialType.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.RawMaterialType entity)
        {
            var existing = await _dbContext.RawMaterialType
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // RawMaterialTypeCode is immutable — never copied here
            existing.RawMaterialTypeName = entity.RawMaterialTypeName;
            existing.Description = entity.Description;
            existing.EffectiveFrom = entity.EffectiveFrom;
            existing.EffectiveTo = entity.EffectiveTo;
            existing.IsActive = entity.IsActive;

            _dbContext.RawMaterialType.Update(existing);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.RawMaterialType
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.RawMaterialType.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
