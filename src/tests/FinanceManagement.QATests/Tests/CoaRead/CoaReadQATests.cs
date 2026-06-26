namespace FinanceManagement.QATests.Tests.CoaRead;

// ─────────────────────────────────────────────────────────────────────────────
// COA Read API for downstream modules (US-GL02-16, live-server QA).
// Route: api/finance/coa
//   GET /accounts/by-code/{code} · GET /accounts?accountTypeId=&accountGroupId=&activeOnly=
//   GET /accounts/validate-for-posting?accountCode=&currencyId=&costCentreId=
// Read-only; authenticated; company from session.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CoaReadCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CoaReadQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa";

    public CoaReadQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // ── get-by-code (AC1) ───────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetByCode_Reachable_Returns200()
    {
        // Unknown code still returns 200 with isSuccess=false/data=null (downstream miss, not an error).
        var resp = await _f.Client.GetAsync($"{Route}/accounts/by-code/QA-UNKNOWN");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetByCode_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/accounts/by-code/QA-UNKNOWN");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── search by type/group (AC5) ──────────────────────────────────────────────
    [Fact, TestPriority(3)]
    [Trait("Layer", "Smoke")]
    public async Task TC003_Search_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}/accounts?activeOnly=false");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Search_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/accounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── validate-for-posting (AC2) ──────────────────────────────────────────────
    [Fact, TestPriority(5)]
    [Trait("Layer", "Smoke")]
    public async Task TC005_Validate_Reachable_Returns200_WithResult()
    {
        var resp = await _f.Client.GetAsync($"{Route}/accounts/validate-for-posting?accountCode=QA-UNKNOWN");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.TryGetProperty("isValid", out _).Should().BeTrue();
        data.TryGetProperty("reasons", out _).Should().BeTrue();
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Validate_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/accounts/validate-for-posting?accountCode=X");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
