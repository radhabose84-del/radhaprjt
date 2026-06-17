using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.CurrencyForexConfig
{
    public class CurrencyForexConfigCommandRepository : ICurrencyForexConfigCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CurrencyForexConfigCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.CurrencyForexConfig entity)
        {
            await _applicationDbContext.CurrencyForexConfig.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.CurrencyForexConfig entity)
        {
            var existingEntity = await _applicationDbContext.CurrencyForexConfig
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // CurrencyTypeCode + CompanyId are immutable
            existingEntity.CurrencyTypeName = entity.CurrencyTypeName;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.CurrencyForexConfig.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.CurrencyForexConfig
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.CurrencyForexConfig.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
