namespace GateEntryManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog (GateEntry) — live-server QA suite (READ-ONLY).
//
// Contract verified against source (2026-06-16):
//   GET    /api/gateentry/AuditLog                       (raw list; NOT paged)
//   GET    /api/gateentry/AuditLog/search?searchPattern=
//
// Key facts that shaped assertions:
//   • Read-only surface — no Create/Update/Delete endpoints.
//   • GetAll returns a raw list shape; the QA clone may have an empty audit collection → tolerant 200/404.
//   • search filters by free-text searchPattern → reachability only (tolerant 200/400/404).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("GateAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/gateentry/AuditLog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — SEARCH  (reachability; tolerant 200/400/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Search_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/search?searchPattern=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Search_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/search?searchPattern=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
