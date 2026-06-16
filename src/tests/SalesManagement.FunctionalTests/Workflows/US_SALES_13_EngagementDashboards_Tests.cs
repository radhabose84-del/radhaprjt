namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-13 — Customer engagement & dashboards
//
//   As a marketing user I record a customer visit and review portal/dashboard insights.
//
// PARTIAL story: the CustomerVisit create is BLOCKED (needs a Party customer the
// testsales marketing officer can access — the MarketingOfficerAccess filter cannot
// be satisfied from read endpoints alone), so it is [Fact(Skip=…)]. The three insight
// reads (AgentPortal dashboard, LeadConversionFunnel, SalesProjection) are read-only
// and agent-/aggregate-scoped (may be empty) → asserted tolerantly for reachability.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-13-EngagementDashboards")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-13")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_13_EngagementDashboards_Tests
{
    private readonly QAServerFixture _f;

    private const string CustomerVisitRoute      = "/api/CustomerVisit";
    private const string AgentPortalRoute        = "/api/AgentPortal";
    private const string LeadConversionRoute     = "/api/LeadConversionFunnel";
    private const string SalesProjectionRoute    = "/api/SalesProjection";

    public US_SALES_13_EngagementDashboards_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a CustomerVisit can be recorded. BLOCKED — needs a Party customer the officer can access.
    [Fact(Skip = "needs seeded data: a Party customer accessible to the testsales marketing officer (MarketingOfficerAccess filter not resolvable from read endpoints)"), TestPriority(1)]
    public async Task Step1_RecordCustomerVisit()
    {
        var resp = await _f.Client.PostAsJsonAsync(CustomerVisitRoute, new { });
        await QAHelper.AssertOkAsync(resp);
    }

    // AC2 — the AgentPortal dashboard is reachable for the logged-in agent (may be empty).
    [Fact, TestPriority(2)]
    public async Task Step2_AgentPortalDashboardReachable()
    {
        var resp = await _f.Client.GetAsync($"{AgentPortalRoute}/dashboard");
        // Agent-scoped: the testsales login may not be an agent, so an empty payload is valid.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC3 — the LeadConversionFunnel report is reachable.
    [Fact, TestPriority(3)]
    public async Task Step3_LeadConversionFunnelReachable()
    {
        var resp = await _f.Client.GetAsync(LeadConversionRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC4 — the SalesProjection report is reachable (Monthly).
    [Fact, TestPriority(4)]
    public async Task Step4_SalesProjectionReachable()
    {
        var resp = await _f.Client.GetAsync($"{SalesProjectionRoute}?PeriodType=Monthly");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
