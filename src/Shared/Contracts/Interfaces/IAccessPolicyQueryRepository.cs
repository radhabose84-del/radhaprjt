namespace Contracts.Interfaces;

/// <summary>
/// Minimal repository interface used by <see cref="IAccessPolicyService"/> implementation
/// to evaluate field-value access policies. Full CRUD interface lives in the Application layer.
/// </summary>
public interface IAccessPolicyQueryRepository
{
    /// <summary>Returns true if the user holds a role with BypassDataAccess = 1.</summary>
    Task<bool> CheckBypassAsync(int userId);

    /// <summary>Returns all active RoleIds allocated to the user.</summary>
    Task<IReadOnlyList<int>> GetUserRoleIdsAsync(int userId);

    /// <summary>
    /// Returns the union of allowed value IDs across all supplied roles for a policy code.
    /// Returns null when the policy code has no configuration (treat as unrestricted).
    /// </summary>
    Task<IReadOnlyList<int>?> GetAllowedValueIdsAsync(string policyCode, IEnumerable<int> roleIds);
}
