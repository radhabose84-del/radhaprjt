using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountMaster
{
    public class GlAccountMasterCommandRepository : IGlAccountMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GlAccountMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.GlAccountMaster entity)
        {
            await _applicationDbContext.GlAccountMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.GlAccountMaster entity)
        {
            var existingEntity = await _applicationDbContext.GlAccountMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // AccountCode + CompanyId are immutable
            existingEntity.AccountTypeId = entity.AccountTypeId;
            existingEntity.AccountGroupId = entity.AccountGroupId;
            existingEntity.AccountName = entity.AccountName;
            existingEntity.Description = entity.Description;
            existingEntity.NormalBalanceId = entity.NormalBalanceId;
            existingEntity.CurrencyTypeId = entity.CurrencyTypeId;
            existingEntity.SubLedgerTypeId = entity.SubLedgerTypeId;
            existingEntity.IsCostCentreMandatory = entity.IsCostCentreMandatory;
            existingEntity.IsProfitCentreMandatory = entity.IsProfitCentreMandatory;
            existingEntity.IsTaxRelevant = entity.IsTaxRelevant;
            existingEntity.IsInterCompany = entity.IsInterCompany;
            existingEntity.IsReconciliationRequired = entity.IsReconciliationRequired;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.GlAccountMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GlAccountMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.GlAccountMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
