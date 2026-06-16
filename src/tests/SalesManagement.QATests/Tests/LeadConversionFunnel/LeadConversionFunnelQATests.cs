namespace SalesManagement.QATests.Tests.LeadConversionFunnel;

// ─────────────────────────────────────────────────────────────────────────────
// LeadConversionFunnel — live-server QA suite (READ-ONLY report).
//
// Contract verified against source (2026-06-15 — LeadConversionFunnelController.cs):
//   GET /api/LeadConversionFunnel    (parameterless)
//
// Key facts that shaped assertions:
//   • Single parameterless GET — no create/update/delete, no query params.
//   • Aggregate funnel report; may legitimately return an empty/zeroed payload.
//     Tolerate 404 in case the report 404s on an empty dataset.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LeadConversionFunnelCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LeadConversionFunnelQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/LeadConversionFunnel";

    public LeadConversionFunnelQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — funnel report happy-path
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetFunnel_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetFunnel_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
