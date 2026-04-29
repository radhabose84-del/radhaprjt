using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOrderTypeMaster
{
    public class SalesOrderTypeMasterCommandRepository : ISalesOrderTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesOrderTypeMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesOrderTypeMaster entity)
        {
            await _dbContext.SalesOrderTypeMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesOrderTypeMaster entity)
        {
            var existing = await _dbContext.SalesOrderTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // SoTypeId + TaxTypeId are immutable after create — never updated here
            existing.TypeName = entity.TypeName;
            existing.Description = entity.Description;

            existing.AllowsDispatch = entity.AllowsDispatch;
            existing.RequiresValidity = entity.RequiresValidity;
            existing.AllowZeroPrice = entity.AllowZeroPrice;
            existing.MinPrice = entity.MinPrice;
            existing.MaxPrice = entity.MaxPrice;
            existing.MaxQty = entity.MaxQty;
            existing.AllowPriceOverride = entity.AllowPriceOverride;
            existing.OverrideLimitPercent = entity.OverrideLimitPercent;
            existing.ApprovalRequired = entity.ApprovalRequired;

            existing.CurrencyRequired = entity.CurrencyRequired;
            existing.AllowIGST = entity.AllowIGST;
            existing.CountryMandatory = entity.CountryMandatory;
            existing.DefaultCurrencyId = entity.DefaultCurrencyId;

            existing.IsActive = entity.IsActive;

            _dbContext.SalesOrderTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.SalesOrderTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, cancellationToken);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.SalesOrderTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
