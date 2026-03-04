using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.MovementTypeConfig
{
    public class MovementTypeConfigCommandRepository : IMovementTypeConfigCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public MovementTypeConfigCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.MovementTypeConfig entity)
        {
            await _applicationDbContext.MovementTypeConfig.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.MovementTypeConfig entity)
        {
            var existing = await _applicationDbContext.MovementTypeConfig
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.MovementDescription  = entity.MovementDescription;
            existing.MovementCategoryId   = entity.MovementCategoryId;
            existing.FromStockTypeId      = entity.FromStockTypeId;
            existing.ToStockTypeId        = entity.ToStockTypeId;
            existing.QuantityUpdateFlag   = entity.QuantityUpdateFlag;
            existing.ValueUpdateFlag      = entity.ValueUpdateFlag;
            existing.AccountModifier      = entity.AccountModifier;
            existing.BatchRequiredFlag    = entity.BatchRequiredFlag;
            existing.NegativeStockAllowed = entity.NegativeStockAllowed;
            existing.IsActive             = entity.IsActive;

            _applicationDbContext.MovementTypeConfig.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.MovementTypeConfig
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.MovementTypeConfig.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
