using System.Diagnostics;

namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-15 — COA Listing & Structure Reports (story).
//
//   As a Finance Controller I want a COA listing (hierarchy + attributes + posting count) with PDF
//   export, an account-usage report, and an FS-mapping validation report, so I can review structure
//   and submit a clean COA to auditors.
//
// All endpoints are read-only and reachable live. Performance ACs are asserted with a generous live
// budget (network + shared clone) rather than the raw SLA. AC3's BS-with-balance exclusion needs a
// specific seed (a stale balance-sheet account carrying a balance) and is proven by the integration
// suite (CoaReportQueryRepositoryTests); here we only assert the report runs and is shaped correctly.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-15")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-15")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0215_CoaStructureReports_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-report";

    public US_GL0215_CoaStructureReports_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // Step 1 — AC1: the COA listing renders with hierarchy + attributes.
    [Fact, TestPriority(1)]
    public async Task Step1_Listing_ReturnsHierarchyRows()
    {
        var resp = await _f.Client.GetAsync($"{Route}/listing");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    // Step 2 — AC1/AC5: the listing PDF is produced (application/pdf, %PDF header) within a live budget.
    [Fact, TestPriority(2)]
    public async Task Step2_ListingPdf_IsAuditorReady_AndTimely()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _f.Client.GetAsync($"{Route}/listing/pdf");
        sw.Stop();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf", "AC5 — auditor-ready PDF");
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(4);
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
        sw.Elapsed.TotalSeconds.Should().BeLessThan(30, "AC1 target is <10s; allowing live headroom on the shared clone");
    }

    // Step 3 — AC2: the usage report completes within a live budget and is shaped correctly.
    [Fact, TestPriority(3)]
    public async Task Step3_UsageReport_Completes_AndFlagsCandidates()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _f.Client.GetAsync($"{Route}/account-usage?MonthsSincePosting=12");
        sw.Stop();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array);
        if (data.GetArrayLength() > 0)
            data[0].TryGetProperty("isDeactivationCandidate", out _).Should().BeTrue("AC3 — candidate flag present");
        sw.Elapsed.TotalSeconds.Should().BeLessThan(60, "AC2 target is <30s for 2,000 accounts; live headroom");
    }

    // Step 4 — AC4: FS-mapping validation reports zero unmapped (clean) or lists those remaining.
    [Fact, TestPriority(4)]
    public async Task Step4_FsMappingValidation_ReportsCleanOrLists()
    {
        var resp = await _f.Client.GetAsync($"{Route}/fs-mapping-validation");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        var unmapped = data.GetProperty("unmappedCount").GetInt32();
        var isClean = data.GetProperty("isClean").GetBoolean();
        isClean.Should().Be(unmapped == 0, "AC4 — clean iff zero unmapped; otherwise the list is populated");
        if (!isClean)
            data.GetProperty("unmapped").GetArrayLength().Should().BeGreaterThan(0);
    }

    // AC3 — the balance-sheet-with-balance exclusion needs a specific seed; proven by the integration suite.
    [Fact(Skip = "AC3 BS-with-balance exclusion needs a seeded stale BS account carrying a balance; covered by CoaReportQueryRepositoryTests."), TestPriority(5)]
    public void Step5_BalanceSheetWithBalance_ExcludedFromCandidates() { }
}
