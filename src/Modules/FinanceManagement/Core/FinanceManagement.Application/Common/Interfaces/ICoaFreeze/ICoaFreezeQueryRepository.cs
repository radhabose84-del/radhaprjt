using FinanceManagement.Application.CoaFreeze.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICoaFreeze
{
    public interface ICoaFreezeQueryRepository
    {
        // The freeze row for a company (null when never frozen).
        Task<CoaFreezeStateRow?> GetStateAsync(int companyId, CancellationToken ct);

        // True when BOTH enforcement triggers exist and are enabled (sys.triggers) — feeds "DB Trigger: ACTIVE".
        Task<bool> AreTriggersActiveAsync(CancellationToken ct);

        // Totals for the freeze console cards.
        Task<(int TotalAccounts, int TotalAccountGroups)> GetCoaCountsAsync(int companyId, CancellationToken ct);
    }

    // Raw freeze-row projection (handler enriches with name/role + counts).
    public sealed class CoaFreezeStateRow
    {
        public bool IsFrozen { get; set; }
        public int? FrozenByUserId { get; set; }
        public DateTimeOffset? FrozenOn { get; set; }
        public DateTimeOffset? UnfreezeWindowExpiry { get; set; }
    }
}
