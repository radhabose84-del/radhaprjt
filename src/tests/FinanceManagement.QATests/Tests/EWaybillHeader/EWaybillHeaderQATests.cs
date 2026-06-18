namespace FinanceManagement.QATests.Tests.EWaybillHeader;

// ─────────────────────────────────────────────────────────────────────────────
// EWaybillHeader (live-server QA) — GST e-waybill headers (NIC EWB integration).
//
// Route: api/finance/ewaybillheader
//   GET (list, paged) · GET {id} · GET by-name?term= · POST · PUT · DELETE ?id=
//
// Create needs an e-invoice/transport context + an external NIC e-waybill call — not
// reproducible in QA, so create + lifecycle are SKIPPED. Smoke / auth / empty-body 400 /
// by-name reachability still run live.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("EWaybillHeaderCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class EWaybillHeaderQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/ewaybillheader";

    public EWaybillHeaderQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // ── SMOKE ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, new { eInvoiceHeaderId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_ByName_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── SKIP — create + lifecycle (seed/external-dependent) ───────────────────
    [Fact(Skip = "needs seeded data: e-invoice/transport + external e-waybill")]
    [TestPriority(10)]
    public async Task TC010_Create_FullLifecycle()
    {
        await Task.CompletedTask;
    }
}
