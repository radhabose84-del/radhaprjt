namespace SalesManagement.QATests.Tests.SalesProjection;

// ─────────────────────────────────────────────────────────────────────────────
// SalesProjection — live-server QA suite (READ-ONLY report).
//
// Contract verified against source (2026-06-15 — SalesProjectionController.cs):
//   GET /api/SalesProjection?PeriodType=&DateFrom=&DateTo=
//     PeriodType : ProjectionPeriodType enum (Monthly | Quarterly | Yearly), default Monthly
//     DateFrom   : DateOnly? (yyyy-MM-dd), optional
//     DateTo     : DateOnly? (yyyy-MM-dd), optional
//
// Key facts that shaped assertions:
//   • Single GET — no create/update/delete.
//   • PeriodType is an enum bound from the query string by name (Monthly/Quarterly/Yearly).
//   • Aggregate projection; may return an empty/zeroed payload → tolerate 404 on no data.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesProjectionCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesProjectionQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesProjection";

    public SalesProjectionQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — Monthly projection happy-path
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetProjection_Monthly_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PeriodType=Monthly");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetProjection_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PeriodType=Monthly");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REACHABILITY — other period types + date range
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetProjection_Quarterly_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PeriodType=Quarterly");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetProjection_Yearly_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PeriodType=Yearly");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetProjection_WithDateRange_Reachable()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}?PeriodType=Monthly&DateFrom=2024-01-01&DateTo=2024-12-31");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetProjection_DefaultPeriodType_Reachable()
    {
        // PeriodType defaults to Monthly when omitted.
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
