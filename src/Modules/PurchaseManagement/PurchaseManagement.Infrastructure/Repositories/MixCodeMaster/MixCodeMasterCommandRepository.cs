using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.MixCodeMaster
{
    public class MixCodeMasterCommandRepository : IMixCodeMasterCommandRepository
    {
        private readonly ApplicationDbContext _db;

        public MixCodeMasterCommandRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.MixCodeMaster entity)
        {
            _db.MixCodeMaster.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(PurchaseManagement.Domain.Entities.MixCodeMaster entity)
        {
            // MixCode is immutable — only mutable fields are copied onto the tracked entity.
            var existing = await _db.MixCodeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing is null) return 0;

            existing.MixCodeDesc = entity.MixCodeDesc;
            existing.IsActive = entity.IsActive;

            await _db.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _db.MixCodeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null) return false;

            existing.IsDeleted = IsDelete.Deleted;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
