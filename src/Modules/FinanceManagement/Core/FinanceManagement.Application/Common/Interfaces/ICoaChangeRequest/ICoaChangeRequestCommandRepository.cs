using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest
{
    // US-GL02-08B — write side of the change-request / dual-approval unfreeze workflow.
    // Entities are returned tracked so handlers mutate then SaveChanges (matching the module convention).
    public interface ICoaChangeRequestCommandRepository
    {
        Task AddChangeRequestWithoutSaveAsync(Domain.Entities.CoaChangeRequest entity, CancellationToken ct);
        Task AddUnfreezeRequestWithoutSaveAsync(CoaUnfreezeRequest entity, CancellationToken ct);

        Task<Domain.Entities.CoaChangeRequest?> GetChangeRequestAsync(int id, CancellationToken ct);
        Task<CoaUnfreezeRequest?> GetUnfreezeRequestAsync(int id, CancellationToken ct);
        Task<List<Domain.Entities.CoaChangeRequest>> GetImpactApprovedChangeRequestsAsync(IEnumerable<int> ids, int companyId, CancellationToken ct);

        // G2 auto-capture: while an unfreeze window is open, link a committed COA edit to a pending change
        // request in that window — mark it Committed + Post-Freeze, stamp the committer, and stamp the
        // account's LastPostFreezeChangeOn (AC3). Returns true when a change request was captured.
        Task<bool> TryCapturePostFreezeChangeAsync(
            int companyId, int? accountId, int? accountGroupId, int userId, DateTimeOffset now, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
