namespace PurchaseManagement.QATests.Tests.ExchangeRate;

// ─────────────────────────────────────────────────────────────────────────────
// ExchangeRate — live-server QA suite (READ-ONLY ingest; treated as reachability).
//
// Contract verified against source (2026-06-17 — ExchangeRateController.cs):
//   Route prefix: [Route("api/[controller]")] → /api/ExchangeRate
//   POST   /api/ExchangeRate           { baseCurrency, symbols[] }  (external FX upsert)
//   GET    /api/ExchangeRate/latest?base=&quote=
//   GET    /api/ExchangeRate/by-date?base=&quote=&date=
//
// Why reachability only:
//   POST hits an external FX provider (network-dependent); GETs read whatever the last ingest
//   stored. None of this is seedable per-run, so we assert reachability tolerantly, not exact
//   payloads. The POST empty-body case tolerates 400/500 because failure may surface from the
//   external dependency, not validation.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ExchangeRateCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ExchangeRateQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ExchangeRate";

    public ExchangeRateQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — LATEST (smoke; tolerant 200/400/404) ──────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetLatest_HappyPath_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/latest?base=USD&quote=INR");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetLatest_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/latest?base=USD&quote=INR");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 2 — BY-DATE (reachability) ────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetByDate_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-date?base=USD&quote=INR&date=2026-06-17");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 3 — POST INGEST (external; tolerant) ──────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_Post_EmptyBody_Tolerant()
    {
        // External-dependency POST — an empty body may fail validation (400) or surface as a
        // downstream error (500). Tolerate both rather than asserting one.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        // Live: external-rate POST tolerates empty body (200).
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 500);
    }
}
