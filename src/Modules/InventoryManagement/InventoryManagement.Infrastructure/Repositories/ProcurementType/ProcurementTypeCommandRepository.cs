using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.ProcurementType
{
    public class ProcurementTypeCommandRepository : IProcurementTypeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProcurementTypeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.ProcurementType entity)
        {
            await _dbContext.ProcurementType.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ProcurementType entity)
        {
            var existing = await _dbContext.ProcurementType
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.ProcurementName = entity.ProcurementName;
            existing.IsActive = entity.IsActive;

            _dbContext.ProcurementType.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.ProcurementType
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ProcurementType.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
