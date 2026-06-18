namespace FinanceManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog (live-server QA) — read-only Finance audit log feed.
//
// Route: api/finance/auditlog
//   GET (raw list) · GET /search?searchPattern=
//
// Read-only — no create/update/delete. Smoke tolerates 200/404 (the clone may have no
// Finance audit rows yet); search tolerates 200/400/404. List rejects anonymous (401).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("FinAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/auditlog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

    // ── SMOKE ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync(Route);
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(Route);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── REACHABILITY (search) ─────────────────────────────────────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_Search_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/search?searchPattern=QA");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
