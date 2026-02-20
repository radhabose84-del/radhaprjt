#nullable disable

using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesSegment
{
    public class SalesSegmentCommandRepository : ISalesSegmentCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesSegmentCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesSegment entity)
        {
            await _dbContext.SalesSegment.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesSegment entity)
        {
            var existing = await _dbContext.SalesSegment
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update only mutable fields (composite key is immutable)
            existing.CurrencyId = entity.CurrencyId;
            existing.ValidFrom = entity.ValidFrom;
            existing.SegmentName = entity.SegmentName;
            existing.IsActive = entity.IsActive;

            _dbContext.SalesSegment.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.SalesSegment
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.SalesSegment.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
