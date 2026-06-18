namespace PurchaseManagement.QATests.Tests.PartyMaster;

// ─────────────────────────────────────────────────────────────────────────────
// PartyMaster (Purchase) — live-server QA suite (READ-ONLY lookup; reachability).
//
// Contract verified against source (2026-06-17 — PartyMasterController.cs):
//   Route prefix: [Route("api/purchase/[controller]")] → /api/purchase/PartyMaster
//   GET    /api/purchase/PartyMaster/GetPartyDetails?oldunitCode=&searchPattern=
//
// Why reachability only:
//   This is a read-only party lookup keyed on an oldunitCode + free-text pattern. The clone has
//   no guaranteed oldunitCode, so we assert reachability tolerantly (200/400/404), not payload.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurPartyMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PartyMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/PartyMaster";

    public PartyMasterQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPartyDetails_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetPartyDetails?oldunitCode=01&searchPattern=A");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetPartyDetails_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetPartyDetails?oldunitCode=01&searchPattern=A");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
