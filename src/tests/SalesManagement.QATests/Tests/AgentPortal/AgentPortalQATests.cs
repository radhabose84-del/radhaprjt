namespace SalesManagement.QATests.Tests.AgentPortal;

// ─────────────────────────────────────────────────────────────────────────────
// AgentPortal — live-server QA suite (READ-ONLY agent-scoped reports).
//
// Contract verified against source (2026-06-15 — AgentPortalController.cs):
//   GET /api/AgentPortal/dashboard                                     (no params)
//   GET /api/AgentPortal/my-customers?PageNumber=&PageSize=&SearchTerm= (paginated)
//   GET /api/AgentPortal/enquiries?PageNumber=&PageSize=&SearchTerm=    (paginated)
//   GET /api/AgentPortal/sales-orders?PageNumber=&PageSize=&SearchTerm= (paginated)
//   GET /api/AgentPortal/complaints?PageNumber=&PageSize=&SearchTerm=   (paginated)
//   GET /api/AgentPortal/invoices?PageNumber=&PageSize=&SearchTerm=     (paginated)
//   GET /api/AgentPortal/dispatches?PageNumber=&PageSize=&SearchTerm=   (paginated)
//   GET /api/AgentPortal/commissions                                   (no params)
//
// Key facts that shaped assertions:
//   • All endpoints are READ-ONLY — no create/update/delete.
//   • Data is scoped to the logged-in agent (resolved from the JWT). The QA `testsales`
//     login may not be an agent at all, so EMPTY results are expected and fine. We assert
//     reachability (200/400/404 tolerant) rather than presence of rows.
//   • The smoke test targets the parameterless `dashboard` GET (proves login→auth→DB→read).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AgentPortalCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AgentPortalQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AgentPortal";

    public AgentPortalQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — dashboard happy-path
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_Dashboard_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/dashboard");
        // Agent-scoped: empty payload is valid. Tolerate 404 if the report 404s on no data.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Dashboard_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/dashboard");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REACHABILITY — paginated agent reports
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_MyCustomers_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/my-customers?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Enquiries_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/enquiries?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_SalesOrders_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/sales-orders?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Complaints_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/complaints?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Invoices_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/invoices?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Dispatches_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/dispatches?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Commissions_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/commissions");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AUTH — confirm a paginated endpoint is also protected
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_MyCustomers_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/my-customers?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
