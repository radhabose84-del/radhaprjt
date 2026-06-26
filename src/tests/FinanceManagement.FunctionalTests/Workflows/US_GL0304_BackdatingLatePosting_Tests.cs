using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-04 — Backdating Controls & Late-Posting Report (workflow / story).
//
//   As a Finance Controller I need every backdated journal flagged automatically
//   so the auditor can drill in by date range / period and the CFO gets a weekly
//   summary email instead of a thousand journal lines. The IsBackdated flag is
//   set by the DB (persisted computed column), not by the client — a payload that
//   ships `isBackdated=false` cannot hide an entry from the report.
//
// This is a STORY test (field-level checks live in LatePostingReportQATests).
// The end-to-end posting flow (Steps 4-7) is BLOCKED on the posting handler being
// wired through IBackdateEnforcementService (GL-01 FR-009). Steps 1-3 prove the
// report contract + sort allow-list immediately.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL03-04")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL03-04")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0304_BackdatingLatePosting_Tests
{
    private readonly QAServerFixture _f;
    private const string ReportRoute = "/api/finance/Journal/late-posting-report";

    public US_GL0304_BackdatingLatePosting_Tests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 1 (AC3 — report endpoint exists, auth-gated, returns shape).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task Step1_GetLatePostingReport_HappyPath_Returns200_WithExpectedEnvelope()
    {
        var resp = await _f.Client.GetAsync($"{ReportRoute}?PageNumber=1&PageSize=50");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var root = doc.RootElement;
        root.TryGetProperty("data",        out _).Should().BeTrue("data array must always be present");
        root.TryGetProperty("TotalCount",  out _).Should().BeTrue();
        root.TryGetProperty("PageNumber",  out _).Should().BeTrue();
        root.TryGetProperty("PageSize",    out _).Should().BeTrue();
    }

    [Fact, TestPriority(2)]
    public async Task Step2_GetLatePostingReport_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{ReportRoute}?PageNumber=1&PageSize=10");
        await Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 3 (Sort allow-list — SQLi defence). The Dapper repo concatenates
    // sort fields directly into ORDER BY, so the validator is the safety net.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(3)]
    public async Task Step3_SortBy_OutsideAllowList_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{ReportRoute}?PageNumber=1&PageSize=10&SortBy=h.Id;DROP TABLE x;--");
        await Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 4 (AC1 — DB-computed IsBackdated). End-to-end: post a JE with
    // VoucherDate < today, then GET the report and find it. BLOCKED until the
    // posting handler is wired through IBackdateEnforcementService and we can
    // hit /api/finance/Journal/post via the QA harness.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs posting handler wiring: AC1 — post a JE with a backdated VoucherDate via " +
                 "/api/finance/Journal/post, then GET /late-posting-report and assert that JE appears with " +
                 "IsBackdated = true and DaysBackdated = the gap. Currently the posting handler does not " +
                 "yet call IBackdateEnforcementService (GL-01 FR-009 territory). Unit + integration tests " +
                 "verify the computed column directly; this step proves the user-visible flow once wired."),
     TestPriority(4)]
    public async Task Step4_PostBackdatedJournal_Then_LatePostingReport_ListsIt()
    {
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 5 (AC2 — SoftClosed period requires reason). Same blocker as Step 4 —
    // we need the posting endpoint to invoke the enforcement service.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs posting handler wiring: AC2 — POST /api/finance/Journal/post with VoucherDate " +
                 "inside a SOFTCLOSED period and a null/whitespace backdateReason → must return 400 with " +
                 "the 'Backdate reason is required when posting to a soft-closed period.' message. The " +
                 "decision logic itself is unit-tested in BackdateEnforcementServiceTests; this step " +
                 "proves it is actually invoked at the API boundary."),
     TestPriority(5)]
    public async Task Step5_PostBackdated_IntoSoftClosed_NoReason_Returns400()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "needs posting handler wiring: AC2 happy-half — same as Step 5 but with a valid " +
                 "backdateReason; the post succeeds and the report shows the supplied reason in the " +
                 "BackdateReason column."),
     TestPriority(6)]
    public async Task Step6_PostBackdated_IntoSoftClosed_WithReason_Succeeds_AndReportShowsReason()
    {
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 7 (AC4 — weekly digest email). The Hangfire recurring job is wired
    // (cron "0 8 * * 1" → Mondays 08:00 UTC), but we cannot deterministically
    // trigger a Hangfire schedule from QA without enqueueing the job by name,
    // and a real SMTP run is not desirable in CI. Verification is unit-level.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs SMTP + role grants + Hangfire enqueue-by-name endpoint: AC4 — Mondays 08:00 UTC " +
                 "the WeeklyBackdatedJournalDigestJob fires and sends a per-company digest email to CFO + " +
                 "FC recipients. Unit-tested in BackgroundService.UnitTests.Jobs.WeeklyBackdatedJournalDigestJobTests " +
                 "(skip-safe, dedup, cc, per-company isolation). Functional verification needs the QA clone " +
                 "to expose a manual-trigger endpoint OR a Hangfire admin dashboard reachable from CI."),
     TestPriority(7)]
    public async Task Step7_WeeklyDigest_Fires_SendsOneEmailPerRecipient_WhenBackdatedRowsExist()
    {
        await Task.CompletedTask;
    }
}
