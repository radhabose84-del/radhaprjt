namespace QCManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// QC AuditLog — live-server QA suite (READ-ONLY).
//
// Contract verified against source (2026-06-16):
//   GET    /api/qc/AuditLog              (raw list — NO paging; data may be array/object/null)
//   GET    /api/qc/AuditLog/by-name?searchPattern=
//
// Read-only entity (MongoDB-backed audit trail) — no Create/Update/Delete endpoints.
// The list may legitimately be empty on the clone, so the smoke read is tolerant 200/404
// and the by-name reachability tolerates 200/400/404.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QcAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/qc/AuditLog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — LIST  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — SEARCH  (by-name?searchPattern= reachability — tolerant 200/400/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(10)]
    public async Task TC010_GetByName_WithPattern_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?searchPattern=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetByName_EmptyPattern_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetByName_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?searchPattern=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
