using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountMaster
{
    // SQL-backed per-user favourites + recently-used for the type-ahead (US-GL02-07).
    // One active row per (UserId, CompanyId, GlAccountMasterId); un-star = soft delete, re-star reactivates.
    public class GlAccountUserPrefRepository : IGlAccountUserPrefStore
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITimeZoneService _timeZoneService;

        public GlAccountUserPrefRepository(ApplicationDbContext dbContext, ITimeZoneService timeZoneService)
        {
            _dbContext = dbContext;
            _timeZoneService = timeZoneService;
        }

        // ── Favourites ──
        public async Task<IReadOnlyList<int>> GetFavouriteAccountIdsAsync(int userId, int companyId, CancellationToken ct = default)
        {
            return await _dbContext.GlAccountFavourite
                .Where(f => f.UserId == userId && f.CompanyId == companyId && f.IsDeleted == IsDelete.NotDeleted)
                .Select(f => f.GlAccountMasterId)
                .ToListAsync(ct);
        }

        public async Task AddFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
        {
            var existing = await _dbContext.GlAccountFavourite
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CompanyId == companyId && f.GlAccountMasterId == accountId, ct);

            if (existing == null)
            {
                await _dbContext.GlAccountFavourite.AddAsync(new GlAccountFavourite
                {
                    UserId = userId,
                    CompanyId = companyId,
                    GlAccountMasterId = accountId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, ct);
            }
            else if (existing.IsDeleted == IsDelete.Deleted)
            {
                existing.IsDeleted = IsDelete.NotDeleted;   // re-star reactivates the prior row
                existing.IsActive = Status.Active;
            }
            else
            {
                return; // already an active favourite — no-op
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task RemoveFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
        {
            var existing = await _dbContext.GlAccountFavourite
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CompanyId == companyId
                                       && f.GlAccountMasterId == accountId && f.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing != null)
            {
                existing.IsDeleted = IsDelete.Deleted;      // soft delete (never physical)
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        // ── Recently-used (record-on-select; most-recent first) ──
        public async Task<IReadOnlyList<GlAccountRecentUseItem>> GetRecentAsync(int userId, int companyId, int take, CancellationToken ct = default)
        {
            return await _dbContext.GlAccountRecentUse
                .Where(r => r.UserId == userId && r.CompanyId == companyId && r.IsDeleted == IsDelete.NotDeleted)
                .OrderByDescending(r => r.LastUsedDate)
                .Take(take > 0 ? take : 10)
                .Select(r => new GlAccountRecentUseItem(r.GlAccountMasterId, r.LastUsedDate, r.UseCount))
                .ToListAsync(ct);
        }

        public async Task RecordRecentAsync(int userId, int companyId, int accountId, CancellationToken ct = default)
        {
            var now = _timeZoneService.GetCurrentTime();

            var existing = await _dbContext.GlAccountRecentUse
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CompanyId == companyId && r.GlAccountMasterId == accountId, ct);

            if (existing == null)
            {
                await _dbContext.GlAccountRecentUse.AddAsync(new GlAccountRecentUse
                {
                    UserId = userId,
                    CompanyId = companyId,
                    GlAccountMasterId = accountId,
                    LastUsedDate = now,
                    UseCount = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, ct);
            }
            else
            {
                existing.LastUsedDate = now;
                existing.UseCount += 1;
                if (existing.IsDeleted == IsDelete.Deleted)
                {
                    existing.IsDeleted = IsDelete.NotDeleted;
                    existing.IsActive = Status.Active;
                }
            }

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
