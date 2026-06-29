using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.QATests.Tests.PeriodStatusOverride;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-02 — Reversal workflow: HARDCLOSED → SOFTCLOSED requires CFO + SysAdmin
// dual approval. The 2nd approval auto-flips the period status (no separate Apply).
//
// This suite requires the testsales user to hold BOTH "CFO" and "SysAdmin" roles
// in the QA clone — without that the approval calls will be denied. Approval steps
// guard with Skip rather than failing if 403 / 401 comes back.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PeriodStatusOverrideCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PeriodStatusOverrideQATests
{
    private readonly QAServerFixture _f;
    private const string OverrideRoute = "/api/finance/PeriodStatusOverride";
    private const string StatusRoute   = "/api/finance/FinancialPeriodStatus";
    private const string YearRoute     = "/api/finance/FinancialYearMaster";

    // BLOCKED by the US-GL03-01 refactor (2026-06-26): /api/finance/FinancialYearMaster (create year +
    // auto-generate 13 periods) was removed. FinancialYear moved to UserManagement (/api/FinancialYear)
    // as a plain master with no period generation, and period status moved to Finance.AccountingPeriod.
    // Every test here depends on the old year→periods setup to obtain a transitionable period, so the
    // whole reversal/override workflow can no longer be seeded. Un-skip and rework once the new
    // AccountingPeriod provisioning contract (how periods are created + the routes that expose them) is settled.
    private const string BlockedReason =
        "US-GL03-01 refactor: /api/finance/FinancialYearMaster (year + 13-period auto-generation) removed; " +
        "FinancialYear moved to UserManagement (/api/FinancialYear) as a plain master and periods moved to " +
        "Finance.AccountingPeriod. Needs rework against the new period-provisioning contract.";

    // Run-unique year in band 8100-8999 (900 slots) — disjoint from FinancialYearMasterQATests
    // (2100-5099) and FinancialPeriodStatusQATests (5100-8099) so the three year-creating suites
    // never pick the same fiscal year on a shared clone, and wide enough to stay re-runnable without
    // a DB reset (a % 100 left only 100 years that fill up and collide — the old "+7" shift kept the
    // 100-slot space and still collided).
    private int StartYear => 8100 + (RunUniqueInt(_f.EntityCode) % 900);
    private string Code         => $"{StartYear}-{(StartYear + 1) % 100:D2}";
    private string StartDateStr => $"{StartYear}-04-01";
    private string EndDateStr   => $"{StartYear + 1}-03-31";

    private static int _yearId;
    private static int _hardClosedPeriodId;
    private static int _overrideId;

    public PeriodStatusOverrideQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SETUP — create year, hard-close one period so we have something to reverse
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = BlockedReason), TestPriority(1)]
    public async Task TC001_Setup_CreateYear_AndHardClosePeriod()
    {
        var yearResp = await _f.Client.PostAsJsonAsync(YearRoute, new
        {
            financialYearCode = Code,
            startDate         = StartDateStr,
            endDate           = EndDateStr,
            isTransitionYear  = false
        });
        await AssertOkAsync(yearResp);
        _yearId = await GetCreatedIdAsync(yearResp);

        var detail = await _f.Client.GetAsync($"{YearRoute}/{_yearId}");
        using var doc = await ParseAsync(detail);
        var periods = doc.RootElement.GetProperty("data").GetProperty("periods");
        _hardClosedPeriodId = periods[0].GetProperty("id").GetInt32();

        // OPEN → SOFTCLOSED → HARDCLOSED
        var soft = await _f.Client.PostAsync($"{StatusRoute}/{_hardClosedPeriodId}/soft-close", null);
        await AssertOkAsync(soft);

        var hard = await _f.Client.PostAsync($"{StatusRoute}/{_hardClosedPeriodId}/hard-close", null);
        await AssertOkAsync(hard);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — REQUEST REVERSAL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = BlockedReason), TestPriority(10)]
    public async Task TC010_RequestReversal_HardClosedToSoftClosed_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _hardClosedPeriodId,
            targetStatusCode = "SOFTCLOSED",
            requestedReason  = "QA test — audit correction"
        });

        await AssertOkAsync(resp);
        _overrideId = await GetCreatedIdAsync(resp);
        _overrideId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = BlockedReason), TestPriority(11)]
    public async Task TC011_RequestReversal_DuplicatePending_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _hardClosedPeriodId,
            targetStatusCode = "SOFTCLOSED",
            requestedReason  = "second attempt"
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "already in progress");
    }

    [Fact(Skip = BlockedReason), TestPriority(12)]
    public async Task TC012_RequestReversal_InvalidTargetCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _hardClosedPeriodId,
            targetStatusCode = "HARDCLOSED",
            requestedReason  = "wrong direction"
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "OPEN");
    }

    [Theory(Skip = BlockedReason), TestPriority(13)]
    [InlineData(null)]
    [InlineData("")]
    public async Task TC013_RequestReversal_EmptyReason_Returns400(string? reason)
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId         = _hardClosedPeriodId,
            targetStatusCode = "SOFTCLOSED",
            requestedReason  = reason
        });

        await Assert400Async(resp);
    }

    [Fact(Skip = BlockedReason), TestPriority(14)]
    public async Task TC014_RequestReversal_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{OverrideRoute}/request", new
        {
            periodId = _hardClosedPeriodId, targetStatusCode = "SOFTCLOSED", requestedReason = "x"
        });
        await Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — PENDING INBOX
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = BlockedReason), TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPending_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{OverrideRoute}/pending");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact(Skip = BlockedReason), TestPriority(21)]
    public async Task TC021_GetPending_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{OverrideRoute}/pending");
        await Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — INVALID-ROLE / VALIDATION GUARDS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = BlockedReason), TestPriority(30)]
    public async Task TC030_Approve_InvalidRole_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/approve", new
        {
            role = "FinanceManager"
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "CFO");
    }

    [Fact(Skip = BlockedReason), TestPriority(31)]
    public async Task TC031_Approve_EmptyRole_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/approve", new
        {
            role = ""
        });

        await Assert400Async(resp);
    }

    [Fact(Skip = BlockedReason), TestPriority(32)]
    public async Task TC032_Reject_EmptyReason_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/reject", new
        {
            rejectionReason = ""
        });

        await Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — APPROVAL FLOW (live-reconcile dependent)
    // The testsales user must hold CFO + SysAdmin roles in the QA clone for these
    // to land green. Skip if the role grant isn't there — log a BUG comment.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "Live-reconcile: requires testsales to hold the CFO role in QA. Un-skip once the user role grant is in place."), TestPriority(40)]
    public async Task TC040_Approve_CFO_RecordsApproval_KeepsPending()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/approve", new
        {
            role = "CFO"
        });

        await AssertOkAsync(resp);
        await AssertBodyContainsAsync(resp, "Awaiting");

        // Verify period is still HARDCLOSED (single approval doesn't flip)
        var statusResp = await _f.Client.GetAsync($"{StatusRoute}/{_hardClosedPeriodId}");
        using var doc = await ParseAsync(statusResp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("HARDCLOSED");
    }

    [Fact(Skip = "Live-reconcile: requires testsales to hold the SysAdmin role in QA. Un-skip once the user role grant is in place."), TestPriority(41)]
    public async Task TC041_Approve_SysAdmin_TriggersAutoApply_FlipsPeriodToSoftClosed()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{OverrideRoute}/{_overrideId}/approve", new
        {
            role = "SysAdmin"
        });

        await AssertOkAsync(resp);
        await AssertBodyContainsAsync(resp, "flipped");

        // Verify period flipped to SOFTCLOSED
        var statusResp = await _f.Client.GetAsync($"{StatusRoute}/{_hardClosedPeriodId}");
        using var doc = await ParseAsync(statusResp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("SOFTCLOSED");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — HISTORY  (post-flow)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = BlockedReason), TestPriority(50)]
    public async Task TC050_StatusHistory_ContainsAtLeastOneOverride()
    {
        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_hardClosedPeriodId}/history");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Array)
            data.GetArrayLength().Should().BeGreaterThan(0);
    }
}
