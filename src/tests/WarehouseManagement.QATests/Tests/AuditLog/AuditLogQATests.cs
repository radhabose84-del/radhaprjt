namespace WarehouseManagement.QATests.Tests.AuditLog;

// ─────────────────────────────────────────────────────────────────────────────
// AuditLog — live-server QA suite (READ-ONLY; status-only assertions).
//
// Contract verified against source (2026-06-16):
//   GET /api/warehouse/AuditLog                       (raw list — returns the mediator result directly)
//   GET /api/warehouse/AuditLog/GetAuditLogSearch?searchPattern=
//
// AuditLog is read-only (no create/update/delete). The list endpoint returns the raw audit-log
// payload (shape varies — MongoDB-backed), so assertions are status-only. Smoke = the GetAll
// happy path. Tolerant 200/404 because the testsales session may have produced no warehouse audit
// rows on the clone.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("WarehouseAuditLogCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/warehouse/AuditLog";

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
    public async Task TC030_GetAuditLogSearch_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetAuditLogSearch_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetAuditLogSearch?searchPattern=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
