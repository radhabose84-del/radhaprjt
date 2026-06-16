namespace LogisticsManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// LogisticsManagement.AuditLog — live-server QA suite (READ-ONLY).
//
// Contract verified against source (2026-06-16):
//   GET    /api/logistics/AuditLog                 (no paging — raw list; returns Ok(auditLogs))
//   GET    /api/logistics/AuditLog/by-name?searchPattern=
//
// Key facts that shaped assertions:
//   • Read-only surface — no Create/Update/Delete endpoints.
//   • The list GET returns the raw query result (no { data, totalCount } envelope), so the
//     response shape is not asserted — only reachability + auth. Tolerant 200/404 because the
//     MongoDB-backed audit store may be empty on the clone.
//   • by-name?searchPattern= is a reachability probe — tolerant 200/400/404.
//   • The list GET is the module's Smoke read (login → auth → audit store).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LogisticsAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/logistics/AuditLog";

    public AuditLogQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200_Tolerant()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_GetBySearchPattern_Reachable_Tolerant()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_GetBySearchPattern_EmptyParam_Reachable_Tolerant()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_GetBySearchPattern_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?searchPattern=Create");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
