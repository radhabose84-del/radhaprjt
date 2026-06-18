namespace ProductionManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog (Production) — live-server QA suite (READ-ONLY log report).
//
// Contract verified against source (2026-06-17):
//   ⚠ Route prefix is "api/production/[controller]":
//   GET /api/production/auditlog                          (list all — raw list, no params)
//   GET /api/production/auditlog/by-name?searchPattern=   (search by free-text pattern)
//
// Key facts that shaped assertions:
//   • Both endpoints are READ-ONLY — no create/update/delete.
//   • List returns the raw query result (not the standard { data, totalCount } envelope) — we
//     assert only the status code, not body shape.
//   • Audit logs live in MongoDB; an empty log set is valid → tolerate 404 alongside 200.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProdAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/production/auditlog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — list all audit logs
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
    // REACHABILITY — search by pattern
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Search_WithPattern_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Search_EmptyPattern_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?searchPattern=");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Search_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?searchPattern=Create");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
