using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.Item.Templates
{
    public sealed class PutAwayRuleCommandRepository : IPutAwayRuleCommandRepository
    {
        private readonly ApplicationDbContext _db;        
        public PutAwayRuleCommandRepository(
            ApplicationDbContext db)
        {
            _db = db;            
        }


        public Task<bool> ExistsScopeAsync(
            int unitId, int warehouseId, int itemGroupId, int itemCategoryId, int? itemId,
            int? excludeId = null, CancellationToken ct = default)
        {
            var q = _db.Set<PutAwayRule>()
                       .AsNoTracking()
                       .Where(r => r.IsDeleted == IsDelete.NotDeleted
                                && r.UnitId == unitId
                                && r.WarehouseId == warehouseId
                                && r.ItemGroupId == itemGroupId
                                && r.ItemCategoryId == itemCategoryId
                                && r.ItemId == itemId);

            if (excludeId.HasValue) q = q.Where(r => r.Id != excludeId.Value);
            return q.AnyAsync(ct);
        }

        public Task<PutAwayRule?> GetByIdAsync(int id, bool track, CancellationToken ct = default)
            => (track
                ? _db.Set<PutAwayRule>().Include(r => r.Strategies)
                : _db.Set<PutAwayRule>().Include(r => r.Strategies).AsNoTracking())
               .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task AddAsync(PutAwayRule entity, CancellationToken ct = default)
        {
            // sane defaults
            if (entity.IsDeleted == 0) entity.IsDeleted = IsDelete.NotDeleted;
            if (entity.IsActive == 0) entity.IsActive = Status.Active;

            // strategies defaults as well
            foreach (var s in entity.Strategies)
            {
                if (s.IsDeleted == 0) s.IsDeleted = IsDelete.NotDeleted;
                if (s.IsActive == 0) s.IsActive = Status.Active;
            }

            await _db.Set<PutAwayRule>().AddAsync(entity, ct);
        }
         public async Task<int> CreateAsync(PutAwayRule entity, CancellationToken ct = default)
        {
            // Defaults (mirrors your existing pattern)
            if (entity.IsDeleted == 0) entity.IsDeleted = IsDelete.NotDeleted;
            if (entity.IsActive  == 0) entity.IsActive  = Status.Active;

            foreach (var s in entity.Strategies)
            {
                if (s.IsDeleted == 0) s.IsDeleted = IsDelete.NotDeleted;
                if (s.IsActive  == 0) s.IsActive  = Status.Active;
            }

            await _db.Set<PutAwayRule>().AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);     // Id is available after this

            return entity.Id;
        }
        public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

        public Task SoftDeleteAsync(PutAwayRule entity, CancellationToken ct = default)
        {
            entity.IsDeleted = IsDelete.Deleted;
            entity.IsActive = Status.Inactive;

            // mark children
            foreach (var s in entity.Strategies)
            {
                s.IsDeleted = IsDelete.Deleted;
                s.IsActive = Status.Inactive;
            }

            return _db.SaveChangesAsync(ct);
        }
        public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        _db.Set<PutAwayRule>()
           .AnyAsync(r => r.Id == id && r.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);       
        
    }
}
