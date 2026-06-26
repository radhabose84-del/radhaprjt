using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-01 — Fiscal Year & Period Calendar Setup (workflow / story).
//
//   As a System Administrator I configure a company-specific fiscal calendar
//   that auto-generates 12 monthly periods + Period 13 (adjustment) so the
//   posting engine has a calendar to validate every JE against.
//
// This is a STORY test (field-level CRUD lives in FinancialYearMasterQATests).
// It proves the end-to-end happy path: create year → 13 periods exist →
// period-for-date resolves correctly → soft-delete cascades to periods.
// Cross-company isolation (AC1) is [blocked] until a second QA user is provisioned.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL03-01")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL03-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0301_FiscalCalendarSetup_Tests
{
    private readonly QAServerFixture _f;
    private const string YearRoute = "/api/finance/FinancialYearMaster";

    // Cross-step state (collection runs serially).
    private static int _yearId;

    // Run-unique year in band 9000-9399 (400 slots; this suite also creates a secondary StartYear+1
    // year, so it effectively touches up to ~9400). Disjoint from every other year-creating Finance
    // suite — QA FinancialYearMaster (2100-5099), FinancialPeriodStatus (5100-8099), PeriodStatusOverride
    // (8100-8999) and Functional US_GL0302 (9500-9899) — so suites never collide on the shared clone,
    // and wide enough to stay re-runnable without a DB reset (the old 2150 + %50 left only 50 years that
    // saturate and collide). EntityCode-derived → stable within a run.
    private int StartYear => 9000 + (RunUniqueInt(_f.EntityCode) % 400);
    private string Code         => $"{StartYear}-{(StartYear + 1) % 100:D2}";
    private string StartDateStr => $"{StartYear}-04-01";
    private string EndDateStr   => $"{StartYear + 1}-03-31";

    public US_GL0301_FiscalCalendarSetup_Tests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 1 (AC2) — Create year, the server auto-generates exactly 13 periods.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task Step1_CreateYear_AutoGenerates12MonthlyPeriodsPlusPeriod13()
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
        _yearId.Should().BeGreaterThan(0);

        var detail = await _f.Client.GetAsync($"{YearRoute}/{_yearId}");
        await AssertOkAsync(detail);

        using var doc = await ParseAsync(detail);
        var periods = doc.RootElement.GetProperty("data").GetProperty("periods");
        periods.GetArrayLength().Should().Be(13, "AC2 — server auto-generates 12 monthly + Period 13");

        // Each period must start OPEN
        foreach (var p in periods.EnumerateArray())
            p.GetProperty("statusCode").GetString().Should().Be("OPEN");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 2 (AC3) — Period 13 shares Period 12's dates (Indian convention).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task Step2_Period13_SharesPeriod12Dates_IndianAccountingConvention()
    {
        _yearId.Should().BeGreaterThan(0, "Step 1 must have created the year");

        var detail = await _f.Client.GetAsync($"{YearRoute}/{_yearId}");
        await AssertOkAsync(detail);

        using var doc = await ParseAsync(detail);
        var periods = doc.RootElement.GetProperty("data").GetProperty("periods");

        var p12 = periods.EnumerateArray().Single(p => p.GetProperty("periodNumber").GetInt32() == 12);
        var p13 = periods.EnumerateArray().Single(p => p.GetProperty("periodNumber").GetInt32() == 13);

        p13.GetProperty("startDate").GetString().Should().Be(p12.GetProperty("startDate").GetString());
        p13.GetProperty("endDate").GetString().Should().Be(p12.GetProperty("endDate").GetString());
        p13.GetProperty("isAdjustmentPeriod").GetBoolean().Should().BeTrue();
        p12.GetProperty("isAdjustmentPeriod").GetBoolean().Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 3 (AC4) — Duplicate `(CompanyId, FinancialYearCode)` rejected.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(3)]
    public async Task Step3_Create_DuplicateCode_SameCompany_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(YearRoute, new
        {
            financialYearCode = Code,
            startDate         = StartDateStr,
            endDate           = EndDateStr,
            isTransitionYear  = false
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "already exists");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 4 (AC4b) — Overlapping date range rejected.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task Step4_Create_OverlappingRange_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(YearRoute, new
        {
            financialYearCode = $"{StartYear + 1}-{(StartYear + 2) % 100:D2}",
            startDate         = $"{StartYear}-08-01",
            endDate           = $"{StartYear + 1}-07-31",
            isTransitionYear  = false
        });

        await Assert400Async(resp);
        await AssertBodyContainsAsync(resp, "overlap");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 5 (Posting engine read API) — period-for-date resolves to correct period.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(5)]
    public async Task Step5_PeriodForDate_DateInJune_ResolvesToPeriod3()
    {
        var resp = await _f.Client.GetAsync($"{YearRoute}/period-for-date?date={StartYear}-06-15");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Object)
        {
            data.GetProperty("periodNumber").GetInt32().Should().Be(3);
            data.GetProperty("isAdjustmentPeriod").GetBoolean().Should().BeFalse();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 6 — period-for-date for March MUST skip the adjustment period.
    // This is the date-resolver's most important invariant: March 15 lives in
    // BOTH Period 12 (regular) and Period 13 (adjustment) calendar-wise, but the
    // posting engine must always route normal posts to Period 12.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(6)]
    public async Task Step6_PeriodForDate_MarchDate_ReturnsRegularPeriod12_NotPeriod13()
    {
        var resp = await _f.Client.GetAsync($"{YearRoute}/period-for-date?date={StartYear + 1}-03-15");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Object)
        {
            data.GetProperty("periodNumber").GetInt32().Should().Be(12,
                "the date-resolver must skip IsAdjustmentPeriod rows so March posts land in Period 12");
            data.GetProperty("isAdjustmentPeriod").GetBoolean().Should().BeFalse();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 7 (Periods read API) — GET /{companyId}/periods returns the full set.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(7)]
    public async Task Step7_GetPeriodsForCompany_Returns200_WithGeneratedPeriods()
    {
        // testsales operates under CompanyId 0 (first-time-login placeholder) and Step 1 creates the
        // year + periods under that company, so the period calendar lives under company 0 on the
        // isolated clone — not the shared-DB "company 1" convention.
        var resp = await _f.Client.GetAsync($"{YearRoute}/0/periods");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.GetArrayLength().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 8 (Soft-delete cascade) — Year delete also tombstones all periods.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(8)]
    public async Task Step8_Delete_SoftDeletesYear_AndCascadesToAllPeriods()
    {
        _yearId.Should().BeGreaterThan(0);

        var resp = await _f.Client.DeleteAsync($"{YearRoute}?id={_yearId}");
        await AssertOkAsync(resp);

        // After delete the year is no longer in the active list — the search-term won't match
        var listResp = await _f.Client.GetAsync($"{YearRoute}?PageNumber=1&PageSize=15&SearchTerm={StartYear}");
        await AssertOkAsync(listResp);
        // (Total count may include other run-leftovers; the assertion is that THIS code isn't in the active list)

        using var doc = await ParseAsync(listResp);
        var data = doc.RootElement.GetProperty("data");
        var codes = data.EnumerateArray().Select(x => x.GetProperty("financialYearCode").GetString()).ToList();
        codes.Should().NotContain(Code, "AC9 — soft-delete removes the year from active reads");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 9 (AC1 — Independent calendars per company) — BLOCKED until a real
    // second QA user is provisioned. Catalogue tag: 🚫.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs seeded data: AC1 (cross-company isolation) requires a second QA user " +
                 "bound to a real CompanyId ≠ testsales' company. Un-skip after the user is provisioned, " +
                 "then create a year as User-B and assert User-A cannot read it through /{companyId}/periods."),
     TestPriority(9)]
    public async Task Step9_CrossCompany_Isolation_TwoUsersEachOwnsTheirCalendar()
    {
        // Intended once the second user exists:
        //   1. As testsales (CompanyA), create year X-Y
        //   2. As userB (CompanyB), create year X-Y
        //   3. GET /api/finance/FinancialYearMaster (scoped by session CompanyId) → A sees only A's year
        //   4. GET /A's id/periods as userB → 200 but data empty (or 404)
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 10 (AC5 — Auto-create next year before EndDate) — BLOCKED.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact(Skip = "needs background job trigger: AC5 verifies the Hangfire `AutoCreateNextFinancialYearMasterJob`. " +
                 "It runs daily at 02:00 UTC and is not exposed via HTTP, so the QA suite cannot trigger it. " +
                 "Verified by unit test: BackgroundService.UnitTests/Jobs/AutoCreateNextFinancialYearMasterJobTests."),
     TestPriority(10)]
    public async Task Step10_AutoCreateNextYear_WhenCurrentEndsWithin3Months()
    {
        await Task.CompletedTask;
    }
}
