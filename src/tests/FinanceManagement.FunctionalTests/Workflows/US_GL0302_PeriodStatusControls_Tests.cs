using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-02 — Period Status & Posting Controls (workflow / story).
//
//   As a Finance Manager I enforce a 3-state one-way period status
//   (Open → SoftClosed → HardClosed) at both the API and DB layers, so postings
//   are controlled as the close progresses. Reversals are gated behind CFO +
//   SysAdmin dual approval — the 2nd approval auto-flips the period in one
//   transaction.
//
// This is a STORY test (field-level checks live in FinancialPeriodStatusQATests
// and PeriodStatusOverrideQATests). It proves the end-to-end happy path:
//   1. Open → SoftClose → HardClose forward transitions are accepted
//   2. State machine rejects illegal transitions (OPEN→HARDCLOSED skip)
//   3. Reversal flow blocked by single approval, auto-applies on 2nd
//   4. Domain event emitted (verified through /status endpoint reflecting the flip)
//
// Approval steps requiring CFO + SysAdmin roles are [blocked] until the QA user
// holds both — Skip with precise reason rather than silent gap.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL03-02")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL03-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0302_PeriodStatusControls_Tests
{
    private readonly QAServerFixture _f;
    private const string YearRoute     = "/api/finance/FinancialYearMaster";
    private const string StatusRoute   = "/api/finance/FinancialPeriodStatus";
    private const string OverrideRoute = "/api/finance/PeriodStatusOverride";

    // BLOCKED by the US-GL03-01 refactor (2026-06-26): /api/finance/FinancialYearMaster (create year +
    // auto-generate 13 periods) was removed. FinancialYear moved to UserManagement (/api/FinancialYear)
    // as a plain master with no period generation, and period status moved to Finance.AccountingPeriod.
    // The whole soft/hard-close + reversal workflow below depends on the old year→periods setup, so it
    // can't seed a transitionable period anymore. Un-skip and rework once the new AccountingPeriod
    // provisioning contract (how periods are created + the routes that expose them) is settled.
    private const string BlockedReason =
        "US-GL03-01 refactor: /api/finance/FinancialYearMaster (year + 13-period auto-generation) removed; " +
        "FinancialYear moved to UserManagement (/api/FinancialYear) as a plain master and periods moved to " +
        "Finance.AccountingPeriod. Needs rework against the new period-provisioning contract.";

    // Cross-step state — collection runs serially so `static` survives across [TestPriority] steps.
    private static int _yearId;
    private static int _periodForSoftClose;    // we soft-close this one mid-flow
    private static int _periodForHardClose;    // we close this one all the way to HARDCLOSED for the reversal test
    private static int _overrideId;

    // Run-unique year in band 9500-9899 (400 slots). Disjoint from every other year-creating Finance
    // suite — QA FinancialYearMaster (2100-5099), FinancialPeriodStatus (5100-8099), PeriodStatusOverride
    // (8100-8999) and Functional US_GL0301 (9000-~9400) — so suites never collide on the shared clone,
    // and wide enough to stay re-runnable without a DB reset (the old 2160 + %40 left only 40 years that
    // saturate and collide, and overlapped US_GL0301's range).
    private int StartYear => 9500 + (RunUniqueInt(_f.EntityCode) % 400);
    private string Code         => $"{StartYear}-{(StartYear + 1) % 100:D2}";
    private string StartDateStr => $"{StartYear}-04-01";
    private string EndDateStr   => $"{StartYear + 1}-03-31";

    public US_GL0302_PeriodStatusControls_Tests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 1 — Seed: create a year so we have periods to transition.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(1)]
    public async Task Step1_SeedYear_CapturesTwoPeriodIds()
    {
        var resp = await _f.Client.PostAsJsonAsync(YearRoute, new
        {
            financialYearCode = Code,
            startDate         = StartDateStr,
            endDate           = EndDateStr,
            isTransitionYear  = false
        });
        await AssertOkAsync(resp);
        _yearId = await GetCreatedIdAsync(resp);

        var detail = await _f.Client.GetAsync($"{YearRoute}/{_yearId}");
        using var doc = await ParseAsync(detail);
        var periods = doc.RootElement.GetProperty("data").GetProperty("periods");
        _periodForSoftClose = periods[0].GetProperty("id").GetInt32();
        _periodForHardClose = periods[1].GetProperty("id").GetInt32();
        _periodForSoftClose.Should().BeGreaterThan(0);
        _periodForHardClose.Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 2 (State machine — illegal forward) — OPEN → HARDCLOSED is rejected;
    // must go through SOFTCLOSED first.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(2)]
    public async Task Step2_StateMachine_OpenToHardClosed_Skip_Returns400()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_periodForHardClose}/hard-close", null);
        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "Illegal");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 3 (Forward transition — Open → SoftClosed) — happy path.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(3)]
    public async Task Step3_SoftClose_FromOpen_Returns200()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_periodForSoftClose}/soft-close", null);
        await AssertOkAsync(resp);

        // Status endpoint reflects the flip (AC4)
        var statusResp = await _f.Client.GetAsync($"{StatusRoute}/{_periodForSoftClose}");
        using var doc = await ParseAsync(statusResp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("SOFTCLOSED");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 4 (Forward transition — SoftClosed → HardClosed) — full chain on the
    // second period so we have something to reverse later.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(4)]
    public async Task Step4_FullForwardChain_SoftCloseThenHardClose_OnSecondPeriod()
    {
        var soft = await _f.Client.PostAsync($"{StatusRoute}/{_periodForHardClose}/soft-close", null);
        await AssertOkAsync(soft);

        var hard = await _f.Client.PostAsync($"{StatusRoute}/{_periodForHardClose}/hard-close", null);
        await AssertOkAsync(hard);

        var statusResp = await _f.Client.GetAsync($"{StatusRoute}/{_periodForHardClose}");
        using var doc = await ParseAsync(statusResp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("HARDCLOSED");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 5 (State machine — illegal reverse via direct API) — HARDCLOSED cannot
    // be directly soft-closed without going through the override flow.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(5)]
    public async Task Step5_StateMachine_HardClosed_DirectSoftClose_Returns400()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_periodForHardClose}/soft-close", null);
        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "Illegal");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 6 (Reversal request) — Override workflow starts: PENDING row created.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(6)]
    public async Task Step6_RequestReversal_HardClosedToSoftClosed_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _periodForHardClose,
            targetStatusCode = "SOFTCLOSED",
            requestedReason  = "QA functional — audit correction simulation"
        });

        await AssertOkAsync(resp);
        _overrideId = await GetCreatedIdAsync(resp);
        _overrideId.Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 7 (Only-one-pending invariant) — second request blocked.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(7)]
    public async Task Step7_DuplicatePendingOverride_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _periodForHardClose,
            targetStatusCode = "SOFTCLOSED",
            requestedReason  = "second attempt"
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "already in progress");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 8 — Invalid role gracefully rejected (gate-keeper before auto-apply).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(8)]
    public async Task Step8_Approve_InvalidRole_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/approve", new
        {
            role = "FinanceManager"     // not in {CFO, SysAdmin}
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "CFO");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 9 (AC3 — first CFO approval keeps PENDING) — BLOCKED until QA user
    // holds the CFO role.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs role grant: AC3 first half — record CFO approval, period stays HARDCLOSED. " +
                 "Requires testsales (or whatever QA user logs in) to hold the CFO role in the QA clone. " +
                 "Un-skip once the role grant is in place; then verify body contains 'Awaiting' and " +
                 "/status still returns HARDCLOSED."),
     TestPriority(9)]
    public async Task Step9_Approve_CFO_RecordsApproval_PeriodStaysHardClosed()
    {
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 10 (AC3 — second SysAdmin approval auto-flips period) — BLOCKED.
    // The critical security guarantee: the SECOND approval atomically flips
    // the period AND marks the override APPLIED in a single transaction.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs role grant: AC3 second half — SysAdmin approval triggers auto-apply " +
                 "(period flip HARDCLOSED→SOFTCLOSED + override APPLIED in one transaction). " +
                 "Requires testsales to also hold SysAdmin role. Un-skip with the role grant; " +
                 "then verify body contains 'flipped' and /status returns SOFTCLOSED."),
     TestPriority(10)]
    public async Task Step10_Approve_SysAdmin_AfterCFO_AutoApplies_PeriodFlipsToSoftClosed()
    {
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 11 (Self-approval prevention / SoD) — Requester cannot approve.
    // BLOCKED (same constraint as Step 9).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs second user: SoD assertion — the requester user must NOT be able to /approve. " +
                 "Requires a second QA user holding CFO/SysAdmin so the requester (testsales) and the " +
                 "approver are different identities. Un-skip when the user is provisioned."),
     TestPriority(11)]
    public async Task Step11_Requester_CannotSelfApprove_Returns400()
    {
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 12 (Audit history) — Status history endpoint reflects the override
    // chain. Implementable even without role grants — the PENDING override
    // from Step 6 is in the history.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = BlockedReason), TestPriority(12)]
    public async Task Step12_StatusHistory_ContainsThePendingOverride()
    {
        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_periodForHardClose}/history");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Array)
            data.GetArrayLength().Should().BeGreaterThan(0, "the override request from Step 6 should appear");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 13 (AC1 + AC2 — Posting gate) — Hard-closed period blocks postings,
    // soft-closed restricts to Finance Manager+. BLOCKED until JournalLine
    // exists (GL-01 FR-009).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs posting engine: AC1 — HARDCLOSED blocks postings via Finance.JournalLine DB trigger. " +
                 "AC2 — SOFTCLOSED restricts to Finance Manager+. The trigger is created by US-GL03-02 " +
                 "migration but is conditional on Finance.JournalLine existing, which is GL-01 FR-009 territory. " +
                 "Verified by unit tests in PeriodPostingGateTests for the gate logic; trigger itself will be " +
                 "covered by the JournalEntry integration suite when that table lands."),
     TestPriority(13)]
    public async Task Step13_PostingGate_HardClosed_RejectsAllPosts_SoftClosed_RestrictsByRole()
    {
        await Task.CompletedTask;
    }
}
