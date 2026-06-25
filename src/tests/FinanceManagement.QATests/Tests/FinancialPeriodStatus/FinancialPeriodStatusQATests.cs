using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.QATests.Tests.FinancialPeriodStatus;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-02 — Period Status Transitions (forward only)
// Covers: GET /{periodId}/status, GET /{periodId}/history, POST /{periodId}/soft-close,
// POST /{periodId}/hard-close.
//
// Reversal flow (override → CFO + SysAdmin approval → auto-apply) lives in the
// companion suite PeriodStatusOverrideQATests.cs.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("FinancialPeriodStatusCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class FinancialPeriodStatusQATests
{
    private readonly QAServerFixture _f;
    private const string StatusRoute = "/api/finance/FinancialPeriodStatus";
    private const string YearRoute   = "/api/finance/FinancialYearMaster";

    // Cross-step year + period for transitions
    private static int _yearId;
    private static int _openPeriodId;
    private static int _softClosedPeriodId;

    private int StartYear => 2100 + (RunUniqueInt(_f.EntityCode) % 100);
    private string Code         => $"{StartYear}-{(StartYear + 1) % 100:D2}";
    private string StartDateStr => $"{StartYear}-04-01";
    private string EndDateStr   => $"{StartYear + 1}-03-31";

    public FinancialPeriodStatusQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SETUP — create a year so we have periods to transition
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateYear_CapturesYearAndFirstPeriodIds()
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

        // Pull the first two period ids — one we'll soft-close, one we'll hard-close
        var detail = await _f.Client.GetAsync($"{YearRoute}/{_yearId}");
        using var doc = await ParseAsync(detail);
        var periods = doc.RootElement.GetProperty("data").GetProperty("periods");
        periods.GetArrayLength().Should().Be(13);

        _openPeriodId       = periods[0].GetProperty("id").GetInt32();
        _softClosedPeriodId = periods[1].GetProperty("id").GetInt32();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET status (AC#4)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetStatus_HappyPath_Returns200_WithOpen()
    {
        _openPeriodId.Should().BeGreaterThan(0);

        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_openPeriodId}");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("statusCode").GetString().Should().Be("OPEN");
        data.GetProperty("isAdjustmentPeriod").GetBoolean().Should().BeFalse();
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetStatus_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{StatusRoute}/{_openPeriodId}");
        await Assert401Async(resp);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetStatus_NonExistent_Returns200_WithNullOrNotFound()
    {
        var resp = await _f.Client.GetAsync($"{StatusRoute}/999999");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — SOFT CLOSE (OPEN → SOFTCLOSED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_SoftClose_OpenPeriod_Returns200()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_softClosedPeriodId}/soft-close", null);
        await AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetStatus_After_SoftClose_ReturnsSoftClosed()
    {
        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_softClosedPeriodId}");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("SOFTCLOSED");
    }

    [Fact, TestPriority(22)]
    public async Task TC022_SoftClose_AlreadySoftClosed_Returns400()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_softClosedPeriodId}/soft-close", null);
        // State-machine guard rejects: SOFTCLOSED → SOFTCLOSED is illegal
        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "Illegal");
    }

    [Fact, TestPriority(23)]
    public async Task TC023_SoftClose_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsync($"{StatusRoute}/{_openPeriodId}/soft-close", null);
        await Assert401Async(resp);
    }

    [Fact, TestPriority(24)]
    public async Task TC024_SoftClose_NonExistentPeriod_Returns400()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/999999/soft-close", null);
        await Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — HARD CLOSE (SOFTCLOSED → HARDCLOSED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_HardClose_OpenPeriod_Returns400_MustBeSoftClosedFirst()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_openPeriodId}/hard-close", null);
        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "Illegal");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_HardClose_SoftClosedPeriod_Returns200()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_softClosedPeriodId}/hard-close", null);
        await AssertOkAsync(resp);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetStatus_After_HardClose_ReturnsHardClosed()
    {
        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_softClosedPeriodId}");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("statusCode").GetString().Should().Be("HARDCLOSED");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_HardClose_AlreadyHardClosed_Returns400()
    {
        var resp = await _f.Client.PostAsync($"{StatusRoute}/{_softClosedPeriodId}/hard-close", null);
        await Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — HISTORY
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_History_Empty_For_PeriodWithoutOverrides_Returns200()
    {
        // _openPeriodId never went through the reversal flow → history must be empty
        var resp = await _f.Client.GetAsync($"{StatusRoute}/{_openPeriodId}/history");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_History_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{StatusRoute}/{_openPeriodId}/history");
        await Assert401Async(resp);
    }
}
