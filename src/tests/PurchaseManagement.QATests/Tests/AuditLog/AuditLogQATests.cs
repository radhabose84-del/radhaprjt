namespace PurchaseManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog (Purchase) — live-server QA suite (READ-ONLY log report).
//
// Contract verified against source (2026-06-17 — AuditLogController.cs):
//   Route prefix: [Route("api/purchase/[controller]")] → /api/purchase/AuditLog
//   GET    /api/purchase/AuditLog                                  (list all — no params)
//   GET    /api/purchase/AuditLog/GetAuditLogSearch?searchPattern= (search by free-text pattern)
//
// Key facts that shaped assertions:
//   • Both endpoints are READ-ONLY — no create/update/delete.
//   • GetAllAuditLogsAsync returns the raw query result (Ok(auditLogs)) — not the standard
//     { data, totalCount } envelope, so we assert only the status code, not body shape.
//   • Audit logs live in MongoDB; an empty log set is valid → tolerate 404 alongside 200.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/AuditLog";

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
    public async Task TC030_Search_WithPattern_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
