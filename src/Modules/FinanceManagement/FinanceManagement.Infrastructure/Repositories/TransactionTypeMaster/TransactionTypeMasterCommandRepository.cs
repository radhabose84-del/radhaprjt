using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster
{
    public class TransactionTypeMasterCommandRepository : ITransactionTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public TransactionTypeMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.TransactionTypeMaster entity)
        {
            await _applicationDbContext.TransactionTypeMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.TransactionTypeMaster entity)
        {
            var existingEntity = await _applicationDbContext.TransactionTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.UnitId = entity.UnitId;
            existingEntity.ModuleId = entity.ModuleId;
            existingEntity.TypeName = entity.TypeName;
            existingEntity.ShortName = entity.ShortName;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.TransactionTypeMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.TransactionTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.TransactionTypeMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
