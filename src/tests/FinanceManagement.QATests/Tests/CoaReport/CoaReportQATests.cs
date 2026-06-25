namespace FinanceManagement.QATests.Tests.CoaReport;

// ─────────────────────────────────────────────────────────────────────────────
// COA Listing & Structure Reports (US-GL02-15, live-server QA).
// Route: api/finance/coa-report
//   GET /listing · GET /listing/pdf · GET /account-usage · GET /fs-mapping-validation
// Read-only; company from session. No configuration changes produced.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CoaReportCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CoaReportQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-report";

    public CoaReportQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // ── Listing (AC1) ───────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_Listing_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}/listing");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Listing_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/listing");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Listing PDF (AC1/AC5) ────────────────────────────────────────────────────
    [Fact, TestPriority(3)]
    [Trait("Layer", "Smoke")]
    public async Task TC003_ListingPdf_Returns200_ApplicationPdf()
    {
        var resp = await _f.Client.GetAsync($"{Route}/listing/pdf");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_ListingPdf_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/listing/pdf");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Account usage (AC2/AC3) ──────────────────────────────────────────────────
    [Fact, TestPriority(5)]
    [Trait("Layer", "Smoke")]
    public async Task TC005_AccountUsage_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}/account-usage?MonthsSincePosting=12");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_AccountUsage_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/account-usage");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── FS-mapping validation (AC4) ──────────────────────────────────────────────
    [Fact, TestPriority(7)]
    [Trait("Layer", "Smoke")]
    public async Task TC007_FsMappingValidation_Returns200_WithSummary()
    {
        var resp = await _f.Client.GetAsync($"{Route}/fs-mapping-validation");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.TryGetProperty("unmappedCount", out _).Should().BeTrue();
        data.TryGetProperty("isClean", out _).Should().BeTrue();
    }

    [Fact, TestPriority(8)]
    public async Task TC008_FsMappingValidation_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/fs-mapping-validation");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
