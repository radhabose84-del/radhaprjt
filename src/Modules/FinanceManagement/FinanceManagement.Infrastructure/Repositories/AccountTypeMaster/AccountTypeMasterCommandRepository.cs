using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.AccountTypeMaster
{
    public class AccountTypeMasterCommandRepository : IAccountTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AccountTypeMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.AccountTypeMaster entity)
        {
            await _applicationDbContext.AccountTypeMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.AccountTypeMaster entity)
        {
            var existingEntity = await _applicationDbContext.AccountTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.AccountTypeName = entity.AccountTypeName;
            existingEntity.StartCode = entity.StartCode;
            existingEntity.AccountCodeLength = entity.AccountCodeLength;
            existingEntity.SortOrder = entity.SortOrder;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.AccountTypeMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.AccountTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.AccountTypeMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
