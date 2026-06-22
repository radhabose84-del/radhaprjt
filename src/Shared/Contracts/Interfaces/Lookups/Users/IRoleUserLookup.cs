namespace Contracts.Interfaces.Lookups.Users
{
    // Cross-module lookup (UserManagement) resolving role membership and member emails by RoleId.
    // Used by US-GL02-08B to (a) verify a dual-approval caller actually holds the configured CFO /
    // System-Admin role and (b) resolve CFO / FC / Internal-Audit recipients for unfreeze alerts.
    // Name ends in 'Lookup' so the global CachedLookupDecorator caches results.
    public interface IRoleUserLookup
    {
        Task<bool> UserHasRoleAsync(int userId, int roleId, CancellationToken ct = default);
        Task<IReadOnlyList<int>> GetUserIdsByRoleAsync(int roleId, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetEmailsByRoleAsync(int roleId, CancellationToken ct = default);
    }
}
