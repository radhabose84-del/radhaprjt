namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-08B — COA Change-Request & Dual-Approval Unfreeze Workflow (story).
//
//   As a CFO I want post-freeze changes to require a change request with an impact
//   assessment and a dual-approval unfreeze by two distinct people, fully logged,
//   so any change to a sealed COA is authorised and traceable.
//
// Steps that complete the dual approval need the CoaUnfreeze RoleIds mapped (CFO +
// System Admin) and the test login to hold those roles — guarded as [Fact(Skip)]
// until the deployment configures them. AC5 (impact assessment required) and the
// AC3 post-freeze log read shape are verifiable without role config.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-08B")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-08B")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0208B_CoaChangeRequest_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-change-request";

    public US_GL0208B_CoaChangeRequest_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // AC5 — a change request cannot be raised without an impact assessment.
    [Fact, TestPriority(1)]
    public async Task Step1_RaiseChangeRequest_WithoutImpactAssessment_IsRejected()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new
        {
            targetAccountId = 1,
            changeType = "AccountEdit",
            justification = "year-end correction",
            impactAssessment = (string?)null
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, "AC5 — impact assessment is mandatory");
    }

    // AC5 — a valid change request is raised and lands in PendingImpactApproval.
    [Fact, TestPriority(2)]
    public async Task Step2_RaiseChangeRequest_WithImpactAssessment_Succeeds()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new
        {
            targetAccountId = (int?)null,
            targetAccountGroupId = 1,
            changeType = "GroupMove",
            justification = "restructure for FY2026",
            impactAssessment = "No downstream statutory mapping impact; reviewed by GL lead."
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = (await ParseAsync(resp)).RootElement;
        root.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    // AC3 — the post-freeze change log endpoint returns a list (report surface).
    [Fact, TestPriority(3)]
    public async Task Step3_PostFreezeChangeLog_ReturnsReport()
    {
        var resp = await _f.Client.GetAsync($"{Route}/post-freeze-log");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // AC1 — same person cannot give both approvals (needs RoleIds + dual-role login to exercise live).
    [Fact(Skip = "AC1/AC2 need CoaUnfreeze RoleIds mapped (CFO + System Admin) and a test login holding them")]
    [TestPriority(4)]
    public Task Step4_DualApproval_DistinctApprovers_OpensWindow() => Task.CompletedTask;

    // AC4 — incomplete change requests lapse on auto-re-freeze (needs Worker job + window-expiry wait).
    [Fact(Skip = "AC4 needs the 'coa-lapse-expired-requests' Hangfire job running + a real wait for window expiry")]
    [TestPriority(5)]
    public Task Step5_IncompleteRequests_LapseOnReFreeze() => Task.CompletedTask;
}
