namespace Contracts.Dtos.Lookups.Sales;

// Resolved chain row for an Agent's Marketing Officer:
//   AgentId → Sales.OfficerAgent.MarketingOfficerId → AppSecurity.Users (EmpId = MarketingOfficerId)
// yields the MO's UserId + ReportToId for downstream username resolution via IUserLookup.
public sealed class MoChainRow
{
    public int AgentId { get; set; }
    public int? MoUserId { get; set; }
    public int? ReportToId { get; set; }
}
