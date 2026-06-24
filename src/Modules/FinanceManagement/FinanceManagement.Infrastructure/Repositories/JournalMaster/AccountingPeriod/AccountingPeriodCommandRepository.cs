using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.AccountingPeriod
{
    public class AccountingPeriodCommandRepository : IAccountingPeriodCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountingPeriodCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(FinanceManagement.Domain.Entities.AccountingPeriod entity)
        {
            await _dbContext.AccountingPeriod.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(FinanceManagement.Domain.Entities.AccountingPeriod entity)
        {
            var existing = await _dbContext.AccountingPeriod
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // FinancialYearId and PeriodNo are immutable — only mutable fields are updated.
            existing.PeriodName = entity.PeriodName;
            existing.StartDate = entity.StartDate;
            existing.EndDate = entity.EndDate;
            existing.StatusId = entity.StatusId;
            existing.IsActive = entity.IsActive;

            _dbContext.AccountingPeriod.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.AccountingPeriod
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.AccountingPeriod.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
