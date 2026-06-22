namespace FinanceManagement.Application.Common.Interfaces.IGlAccountMaster
{
    // Per-user account preferences for the type-ahead (US-GL02-07): favourites + recently-used.
    // Backed by MongoDB (no SQL table/migration) — keyed by (UserId, CompanyId, AccountId).
    public interface IGlAccountUserPrefStore
    {
        // ── Favourites ──
        Task<IReadOnlyList<int>> GetFavouriteAccountIdsAsync(int userId, int companyId, CancellationToken ct = default);
        Task AddFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default);
        Task RemoveFavouriteAsync(int userId, int companyId, int accountId, CancellationToken ct = default);

        // ── Recently-used (record-on-select; ordered most-recent first) ──
        Task<IReadOnlyList<GlAccountRecentUseItem>> GetRecentAsync(int userId, int companyId, int take, CancellationToken ct = default);
        Task RecordRecentAsync(int userId, int companyId, int accountId, CancellationToken ct = default);
    }

    // A recently-used account id with its last-used timestamp (for ranking).
    public sealed record GlAccountRecentUseItem(int AccountId, DateTimeOffset LastUsedDate, int UseCount);
}
