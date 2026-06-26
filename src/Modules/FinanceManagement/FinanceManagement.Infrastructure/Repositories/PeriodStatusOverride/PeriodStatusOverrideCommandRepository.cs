using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.PeriodStatusOverride
{
    public class PeriodStatusOverrideCommandRepository : IPeriodStatusOverrideCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public PeriodStatusOverrideCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.PeriodStatusOverride entity, CancellationToken ct)
        {
            await _applicationDbContext.PeriodStatusOverride.AddAsync(entity, ct);
            await _applicationDbContext.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.PeriodStatusOverride entity, CancellationToken ct)
        {
            var existing = await _applicationDbContext.PeriodStatusOverride
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return 0;

            existing.CfoApproverId       = entity.CfoApproverId;
            existing.CfoApprovedAt       = entity.CfoApprovedAt;
            existing.SysAdminApproverId  = entity.SysAdminApproverId;
            existing.SysAdminApprovedAt  = entity.SysAdminApprovedAt;
            existing.OverrideStatusId    = entity.OverrideStatusId;
            existing.AppliedAt           = entity.AppliedAt;
            existing.RejectionReason     = entity.RejectionReason;

            _applicationDbContext.PeriodStatusOverride.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return existing.Id;
        }

        public async Task<bool> ApplyPeriodStatusChangeAsync(
            int accountingPeriodId,
            int newStatusId,
            int changedBy,
            DateTimeOffset changedAt,
            int? overrideIdToMarkApplied,
            int? appliedStatusIdForOverride,
            CancellationToken ct)
        {
            // DbContext uses EnableRetryOnFailure, so a user-initiated transaction must run inside the
            // execution strategy (which retries the whole unit on a transient fault).
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _applicationDbContext.Database.BeginTransactionAsync(ct);

                var period = await _applicationDbContext.AccountingPeriod
                    .FirstOrDefaultAsync(p => p.Id == accountingPeriodId && p.IsDeleted == IsDelete.NotDeleted, ct);
                if (period == null) return false;

                period.StatusId            = newStatusId;
                period.LastStatusChangedBy = changedBy;
                period.LastStatusChangedAt = changedAt;
                _applicationDbContext.AccountingPeriod.Update(period);

                if (overrideIdToMarkApplied.HasValue && appliedStatusIdForOverride.HasValue)
                {
                    var ovr = await _applicationDbContext.PeriodStatusOverride
                        .FirstOrDefaultAsync(x => x.Id == overrideIdToMarkApplied.Value && x.IsDeleted == IsDelete.NotDeleted, ct);
                    if (ovr != null)
                    {
                        ovr.OverrideStatusId = appliedStatusIdForOverride.Value;
                        ovr.AppliedAt        = changedAt;
                        _applicationDbContext.PeriodStatusOverride.Update(ovr);
                    }
                }

                await _applicationDbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return true;
            });
        }
    }
}
