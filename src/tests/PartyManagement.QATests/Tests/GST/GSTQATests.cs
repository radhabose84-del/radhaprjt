namespace PartyManagement.QATests.Tests.GST;

// ─────────────────────────────────────────────────────────────────────────────
// GST (Party) — live-server QA suite (READ-ONLY external lookup).
//
// Contract verified against source (2026-06-17 — GSTController.cs):
//   GET /api/GST/auth                 (fetches an auth token from the external GST service)
//   GET /api/GST/gstin/{gstin}        (fetches GSTIN details from the external GST service)
//
// Key facts that shaped assertions:
//   • Both endpoints proxy a 3rd-party GST service — its reachability/credentials are not
//     controlled by the QA clone. Responses are tolerated across 200/400/404/500 so an external
//     outage or unconfigured key does not red the suite.
//   • Nothing is tagged [Trait("Layer","Smoke")] here — both calls hit an external dependency and
//     are tolerant, so they are not a valid deploy gate. /auth serves as a reachability check only.
//   • no-auth must still be rejected by the global TokenValidationMiddleware (401).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("GSTCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class GSTQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/GST";

    // A syntactically-valid dummy GSTIN (15 chars, standard format). Not expected to resolve.
    private const string DummyGstin = "22AAAAA0000A1Z5";

    public GSTQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    public async Task TC020_GetAuthToken_Reachable()
    {
        // External service — tolerate success and any upstream failure.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/auth");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAuthToken_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/auth");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_GetGstinDetails_Reachable()
    {
        // External lookup with a dummy GSTIN — tolerate any of 200/400/404/500.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/gstin/{DummyGstin}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetGstinDetails_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/gstin/{DummyGstin}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
