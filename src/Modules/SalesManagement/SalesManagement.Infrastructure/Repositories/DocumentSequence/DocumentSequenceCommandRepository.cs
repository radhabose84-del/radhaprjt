using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Domain.Common;
using SalesManagement.Infrastructure.Data;

namespace SalesManagement.Infrastructure.Repositories.DocumentSequence
{
    public class DocumentSequenceCommandRepository : IDocumentSequenceCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DocumentSequenceCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.DocumentSequence entity)
        {
            await _dbContext.DocumentSequence.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.DocumentSequence entity)
        {
            var existing = await _dbContext.DocumentSequence
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.TypeId = entity.TypeId;
            existing.FinancialYearId = entity.FinancialYearId;
            existing.DocNo = entity.DocNo;
            existing.IsActive = entity.IsActive;

            _dbContext.DocumentSequence.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.DocumentSequence
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = BaseEntity.IsDelete.Deleted;
            _dbContext.DocumentSequence.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
