namespace SalesManagement.Application.Common.Interfaces
{
    /// <summary>
    /// Centralized access filter for Marketing Officer data scoping.
    /// When the current user has <c>EmpId</c> set on their JWT, they are treated as a Marketing Officer
    /// and may only access data for the agents and customers mapped to them via
    /// <c>OfficerAgent</c> and <c>AgentCustomerMapping</c>.
    /// Users without <c>EmpId</c> (admin/superuser) bypass the filter entirely.
    /// </summary>
    public interface IMarketingOfficerAccessFilter
    {
        /// <summary>True when the current user has <c>EmpId</c> set (i.e., is a Marketing Officer).</summary>
        bool IsMarketingOfficer();

        /// <summary>Current user's MarketingOfficer.Id, or null for admin/superuser.</summary>
        int? GetCurrentMarketingOfficerId();

        /// <summary>
        /// Distinct AgentIds currently assigned to the officer (validity-aware).
        /// Empty list if not a Marketing Officer.
        /// </summary>
        Task<IReadOnlyList<int>> GetAccessibleAgentIdsAsync(CancellationToken ct = default);

        /// <summary>
        /// Distinct CustomerIds reachable via the officer's assigned agents.
        /// Empty list if not a Marketing Officer.
        /// </summary>
        Task<IReadOnlyList<int>> GetAccessibleCustomerIdsAsync(CancellationToken ct = default);

        /// <summary>True if admin OR <paramref name="customerId"/> is in the officer's accessible customer list.</summary>
        Task<bool> CanAccessCustomerAsync(int customerId, CancellationToken ct = default);

        /// <summary>True if admin OR <paramref name="agentId"/> is in the officer's accessible agent list.</summary>
        Task<bool> CanAccessAgentAsync(int agentId, CancellationToken ct = default);

        /// <summary>
        /// Returns true when marketing officer data filtering should be applied.
        /// Returns false when:
        /// <list type="bullet">
        ///   <item>User has no <c>EmpId</c> (admin/superuser)</item>
        ///   <item>User's role has <c>BypassDataAccess = true</c></item>
        /// </list>
        /// Use this instead of <see cref="IsMarketingOfficer()"/> when deciding whether to apply
        /// data access restrictions. <see cref="IsMarketingOfficer()"/> only checks if <c>EmpId</c>
        /// is present but does not consider role-level bypass.
        /// </summary>
        Task<bool> ShouldApplyFilterAsync(CancellationToken ct = default);
    }
}
