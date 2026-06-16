namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-12 — Complaint to resolution & return
//
//   As a service user I log a complaint, run QC review, collect department feedback,
//   resolve it, and process a sales return.
//
// Live-reconciled status:
//   • Reachability reads are active (Complaint + SalesReturn list endpoints) plus a
//     no-auth 401 check — they prove the after-sales read path works.
//   • BLOCKED: the complaint-to-return chain needs a customer with posted invoice
//     lines, then a complaint past QC and a resolution before a return can be
//     processed. The QA clone has no invoiced-customer scope, so the create ACs are
//     [Fact(Skip)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-12-ComplaintToReturn")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-12")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_12_ComplaintToReturn_Tests
{
    private readonly QAServerFixture _f;

    private const string ComplaintRoute   = "/api/Complaint";
    private const string SalesReturnRoute = "/api/SalesReturn";

    public US_SALES_12_ComplaintToReturn_Tests(QAServerFixture fixture) => _f = fixture;

    // Reachability — the complaint list answers for an authenticated user
    // (tolerant: 200 with data, or 404 when empty on the clone).
    [Fact, TestPriority(1)]
    public async Task Step1_ComplaintListReachable()
    {
        var resp = await _f.Client.GetAsync($"{ComplaintRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the sales-return list answers for an authenticated user.
    [Fact, TestPriority(2)]
    public async Task Step2_SalesReturnListReachable()
    {
        var resp = await _f.Client.GetAsync($"{SalesReturnRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Security — the complaint list rejects an unauthenticated caller (401).
    [Fact, TestPriority(3)]
    public async Task Step3_ComplaintListRequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{ComplaintRoute}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    // AC1 — a Complaint can be logged for a customer with invoice lines.
    [Fact(Skip = "needs seeded data: requires a customer with posted invoice lines — no invoiced-customer scope on BannariERP_QATest."), TestPriority(4)]
    public Task Step4_LogComplaint() => Task.CompletedTask;

    // AC2 — a ComplaintQCReview can be submitted with department assignments.
    [Fact(Skip = "needs seeded data: requires a logged Complaint (blocked by Step4)."), TestPriority(5)]
    public Task Step5_SubmitQCReview() => Task.CompletedTask;

    // AC3 — ComplaintDepartmentFeedback (RCA) can be submitted per assignment.
    [Fact(Skip = "needs seeded data: requires a QC review department assignment (blocked by Step5)."), TestPriority(6)]
    public Task Step6_SubmitDepartmentFeedback() => Task.CompletedTask;

    // AC4 — a ComplaintResolution of type 'Sales Return' can be recorded.
    [Fact(Skip = "needs seeded data: requires a Complaint past QC (blocked by Step5–Step6)."), TestPriority(7)]
    public Task Step7_RecordResolution() => Task.CompletedTask;

    // AC5 — a SalesReturn can be processed against that resolution.
    [Fact(Skip = "needs seeded data: requires a ComplaintResolution + invoice pack ranges (blocked by Step7)."), TestPriority(8)]
    public Task Step8_ProcessSalesReturn() => Task.CompletedTask;
}
