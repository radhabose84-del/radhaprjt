namespace FinanceManagement.Application.Common.Interfaces.ICoaFreeze
{
    // Freeze-state transitions. In 08a these back a TEST-ONLY hook; the governed (dual-approval)
    // freeze/unfreeze is US-GL02-08B, which calls the same store. Auto-re-freeze (system) is the
    // BackgroundService Hangfire job. All upsert the single freeze row per company.
    public interface ICoaFreezeCommandRepository
    {
        // Seal the COA: IsFrozen = 1, FrozenOn = now, clear any open window.
        Task FreezeAsync(int companyId, int frozenByUserId, DateTimeOffset frozenOn, CancellationToken ct);

        // Open an unfreeze window: IsFrozen = 0, UnfreezeWindowExpiry = expiry (FrozenOn kept).
        Task OpenUnfreezeWindowAsync(int companyId, DateTimeOffset expiry, CancellationToken ct);
    }
}
