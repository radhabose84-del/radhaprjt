using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales;

public interface IOfficerAgentUserLookup
{
    /// <summary>
    /// Resolves the Marketing Officer's UserId for the given Agent
    /// (Sales.OfficerAgent.MarketingOfficerId → AppSecurity.Users.EmpId → UserId).
    /// Used at Sales Order creation to route the InApp to the agent's specific MO.
    /// </summary>
    Task<int?> GetMarketingOfficerUserIdByAgentIdAsync(int agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the ReportTo UserId for a given user. Used at the next workflow step:
    /// when the MO (or any approver) approves, fetch their ReportToId so the next-level
    /// notification can be routed to the manager. The userId argument is typically
    /// parsed from the JWT 'nameid' claim of the user who just approved.
    /// </summary>
    Task<int?> GetMarketingOfficerReportToUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk variant: resolves the full Marketing Officer chain (MO UserId + ReportToId) for many Agents.
    /// Returns one row per AgentId that has an active OfficerAgent + active matching User.
    /// </summary>
    Task<IReadOnlyList<MoChainRow>> GetMarketingOfficerChainByAgentIdsAsync(
        IEnumerable<int> agentIds, CancellationToken cancellationToken = default);
}
