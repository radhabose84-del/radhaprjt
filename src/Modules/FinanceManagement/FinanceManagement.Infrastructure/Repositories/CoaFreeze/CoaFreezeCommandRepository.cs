using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.CoaFreeze
{
    public class CoaFreezeCommandRepository : ICoaFreezeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoaFreezeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task FreezeAsync(int companyId, int frozenByUserId, DateTimeOffset frozenOn, CancellationToken ct)
        {
            var row = await GetOrCreateAsync(companyId, ct);
            row.IsFrozen = true;
            row.FrozenByUserId = frozenByUserId;
            row.FrozenOn = frozenOn;
            row.UnfreezeWindowExpiry = null;
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task OpenUnfreezeWindowAsync(int companyId, DateTimeOffset expiry, CancellationToken ct)
        {
            var row = await GetOrCreateAsync(companyId, ct);
            row.IsFrozen = false;                 // FrozenOn kept (history); the timer re-freezes on expiry
            row.UnfreezeWindowExpiry = expiry;
            await _dbContext.SaveChangesAsync(ct);
        }

        private async Task<CoaFreezeState> GetOrCreateAsync(int companyId, CancellationToken ct)
        {
            var row = await _dbContext.CoaFreezeState
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (row == null)
            {
                row = new CoaFreezeState { CompanyId = companyId };
                await _dbContext.CoaFreezeState.AddAsync(row, ct);
            }
            return row;
        }
    }
}
