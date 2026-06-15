namespace Contracts.Interfaces;

public interface IAccessPolicyService
{
    /// <summary>
    /// Returns allowed value IDs for the given policy code scoped to the current user's roles.
    /// <para>Returns <c>null</c>  — bypass (admin/super-admin) or policy not configured; consumer treats as unrestricted.</para>
    /// <para>Returns empty list — policy is configured but no values assigned to the user's roles; consumer must deny.</para>
    /// </summary>
    Task<IReadOnlyList<int>?> GetAllowedValueIdsAsync(
        string policyCode,
        CancellationToken ct = default);
}
