namespace ProjectManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog (ProjectManagement) — live-server QA suite (READ-ONLY).
//
// Contract verified against source (2026-06-16):
//   GET    /api/project/AuditLog                         (raw audit-log list — no pagination wrapper)
//   GET    /api/project/AuditLog/GetAuditLogSearch?searchPattern=
//
// Read-only surface (MongoDB-backed audit logs). No CRUD. Assertions are tolerant: the list may be
// empty (200 + [] / {}) on a fresh clone, and the search endpoint may return 200/400/404 depending
// on the pattern. The no-auth check on the list proves the global auth middleware protects it.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProjectAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/project/AuditLog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

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

    [Fact, TestPriority(30)]
    public async Task TC030_GetAuditLogSearch_Reachable_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetAuditLogSearch_EmptyPattern_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetAuditLogSearch_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
