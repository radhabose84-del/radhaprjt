namespace FinanceManagement.QATests.Tests.CoaChangeRequest;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-08B — COA change-request + dual-approval unfreeze workflow (live-server QA).
//
// Route: api/finance/coa-change-request
//   GET  /                       → list change requests (paged, status filter)
//   POST /                       → raise a change request (impact assessment required — AC5)
//   POST /approve-impact         → CFO approves impact (role-gated)
//   GET  /post-freeze-log        → AC3 post-freeze change log
//   POST /seal                   → governed CFO seal (G1)
//   POST /unfreeze               → raise dual-approval unfreeze request
//   POST /unfreeze/approve       → record one approval (AC1/AC2)
//   GET  /unfreeze/{id}          → unfreeze request status
//
// Auth + validation + read shapes are asserted here. Steps that require the
// CoaUnfreeze RoleIds to be configured (CFO/SysAdmin) + a completed dual approval
// are guarded as [Fact(Skip)] until the deployment maps those roles.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CoaChangeRequestCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CoaChangeRequestQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-change-request";

    public CoaChangeRequestQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200WithList()
    {
        var resp = await _f.Client.GetAsync(Route);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(Route);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, new { changeType = "AccountEdit" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingImpactAssessment_Returns400()
    {
        // AC5 — the impact assessment is mandatory at raise time.
        var resp = await _f.Client.PostAsJsonAsync(Route, new
        {
            targetAccountId = 1,
            changeType = "AccountEdit",
            justification = "year-end correction",
            impactAssessment = (string?)null
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(5)]
    [Trait("Layer", "Smoke")]
    public async Task TC005_PostFreezeLog_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}/post-freeze-log");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_ApproveUnfreeze_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{Route}/unfreeze/approve", new { unfreezeRequestId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Needs CoaUnfreeze RoleIds configured (CFO/SysAdmin) + an impact-approved change request seeded")]
    [TestPriority(7)]
    public Task TC007_FullDualApprovalFlow_OpensWindow() => Task.CompletedTask;
}
