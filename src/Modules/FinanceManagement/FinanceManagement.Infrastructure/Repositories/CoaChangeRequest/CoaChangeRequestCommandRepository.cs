using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.CoaChangeRequest
{
    public class CoaChangeRequestCommandRepository : ICoaChangeRequestCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoaChangeRequestCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddChangeRequestWithoutSaveAsync(Domain.Entities.CoaChangeRequest entity, CancellationToken ct)
            => await _dbContext.CoaChangeRequest.AddAsync(entity, ct);

        public async Task AddUnfreezeRequestWithoutSaveAsync(CoaUnfreezeRequest entity, CancellationToken ct)
            => await _dbContext.CoaUnfreezeRequest.AddAsync(entity, ct);

        public async Task<Domain.Entities.CoaChangeRequest?> GetChangeRequestAsync(int id, CancellationToken ct)
            => await _dbContext.CoaChangeRequest
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        public async Task<CoaUnfreezeRequest?> GetUnfreezeRequestAsync(int id, CancellationToken ct)
            => await _dbContext.CoaUnfreezeRequest
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        public async Task<List<Domain.Entities.CoaChangeRequest>> GetImpactApprovedChangeRequestsAsync(
            IEnumerable<int> ids, int companyId, CancellationToken ct)
        {
            var idList = ids.Distinct().ToList();
            return await _dbContext.CoaChangeRequest
                .Where(x => idList.Contains(x.Id)
                            && x.CompanyId == companyId
                            && x.RequestStatus == CoaChangeRequestStatus.ImpactApproved
                            && x.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync(ct);
        }

        public async Task<bool> TryCapturePostFreezeChangeAsync(
            int companyId, int? accountId, int? accountGroupId, int userId, DateTimeOffset now, CancellationToken ct)
        {
            // Only capture while a window is genuinely open (not yet auto-re-frozen).
            var openWindow = await _dbContext.CoaUnfreezeRequest
                .Where(u => u.CompanyId == companyId
                            && u.RequestStatus == CoaUnfreezeRequestStatus.WindowOpen
                            && u.IsDeleted == IsDelete.NotDeleted
                            && u.WindowExpiry != null && u.WindowExpiry > now)
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync(ct);

            if (openWindow == null)
                return false;

            // Candidate change requests in this window not yet committed/lapsed.
            var candidates = await _dbContext.CoaChangeRequest
                .Where(c => c.UnfreezeRequestId == openWindow.Id
                            && c.RequestStatus == CoaChangeRequestStatus.ImpactApproved
                            && c.IsDeleted == IsDelete.NotDeleted)
                .OrderBy(c => c.Id)
                .ToListAsync(ct);

            if (candidates.Count == 0)
                return false;

            // Prefer the request whose declared target matches the actual edit; else fall back to the
            // oldest open request (heuristic linkage — see G2). Best-effort, never throws.
            var match = candidates.FirstOrDefault(c =>
                            (accountId.HasValue && c.TargetAccountId == accountId) ||
                            (accountGroupId.HasValue && c.TargetAccountGroupId == accountGroupId))
                        ?? candidates[0];

            match.RequestStatus = CoaChangeRequestStatus.Committed;
            match.IsPostFreeze = true;
            match.CommittedByUserId = userId;
            match.CommittedOn = now;
            _dbContext.CoaChangeRequest.Update(match);

            // Stamp the account so COA listings/exports mark it 'Post-Freeze' (AC3 / G3).
            var stampAccountId = accountId ?? match.TargetAccountId;
            if (stampAccountId.HasValue)
            {
                var account = await _dbContext.GlAccountMaster
                    .FirstOrDefaultAsync(a => a.Id == stampAccountId.Value && a.IsDeleted == IsDelete.NotDeleted, ct);
                if (account != null)
                {
                    account.LastPostFreezeChangeOn = now;
                    _dbContext.GlAccountMaster.Update(account);
                }
            }

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task SaveChangesAsync(CancellationToken ct)
            => await _dbContext.SaveChangesAsync(ct);
    }
}
