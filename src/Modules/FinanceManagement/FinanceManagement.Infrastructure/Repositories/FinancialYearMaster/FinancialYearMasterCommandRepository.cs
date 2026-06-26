using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.FinancialYearMaster
{
    public class FinancialYearMasterCommandRepository : IFinancialYearMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public FinancialYearMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(
            Domain.Entities.FinancialYearMaster year,
            IReadOnlyList<Domain.Entities.FinancialPeriodMaster> periods,
            CancellationToken ct)
        {
            // DbContext uses EnableRetryOnFailure, so a user-initiated transaction must run inside the
            // execution strategy (which retries the whole unit on a transient fault).
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _applicationDbContext.Database.BeginTransactionAsync(ct);

                await _applicationDbContext.FinancialYearMaster.AddAsync(year, ct);
                await _applicationDbContext.SaveChangesAsync(ct);

                // Wire each period to the freshly-generated header id, then bulk-insert
                foreach (var p in periods)
                    p.FinancialYearId = year.Id;

                await _applicationDbContext.FinancialPeriodMaster.AddRangeAsync(periods, ct);
                await _applicationDbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return year.Id;
            });
        }

        public async Task<int> UpdateAsync(Domain.Entities.FinancialYearMaster entity)
        {
            var existing = await _applicationDbContext.FinancialYearMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // StartDate / EndDate / StatusId are immutable in this story
            existing.FinancialYearCode = entity.FinancialYearCode;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.FinancialYearMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.FinancialYearMaster
                .Include(fy => fy.Periods)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            if (existing.Periods != null)
            {
                foreach (var p in existing.Periods)
                    p.IsDeleted = IsDelete.Deleted;
            }

            _applicationDbContext.FinancialYearMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
