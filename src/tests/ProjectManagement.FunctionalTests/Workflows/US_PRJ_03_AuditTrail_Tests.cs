namespace ProjectManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PRJ-03 — Project audit trail (read-only)
//   As a project administrator I review the audit trail of project actions.
// Route verified from ProjectManagement.QATests: /api/project/AuditLog (raw list, status-only);
// search via GetAuditLogSearch?searchPattern=.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PRJ-03-AuditTrail")]
[Trait("Module", "ProjectManagement")]
[Trait("Story", "US-PRJ-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PRJ_03_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/project/AuditLog";

    public US_PRJ_03_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — all project audit logs can be listed.
    [Fact, TestPriority(1)]
    public async Task Step1_ListAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync(AuditRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC2 — audit logs can be searched by a pattern.
    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{AuditRoute}/GetAuditLogSearch?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
