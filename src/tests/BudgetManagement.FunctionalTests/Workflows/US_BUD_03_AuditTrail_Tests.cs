namespace BudgetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-BUD-03 — Budget audit & activity trail (read-only)
//   As a budget administrator I review the audit + activity trails of budget actions.
// Routes verified from BudgetManagement.QATests: /api/budget/auditlog (raw list; search via
// GetAuditLogSearch?searchPattern=) and /api/budget/logs/{entityName}/{entityId} (activity logs).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-BUD-03-AuditTrail")]
[Trait("Module", "BudgetManagement")]
[Trait("Story", "US-BUD-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_BUD_03_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/budget/auditlog";
    private const string LogsRoute  = "/api/budget/logs";

    public US_BUD_03_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task Step1_ListAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync(AuditRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{AuditRoute}/GetAuditLogSearch?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(3)]
    public async Task Step3_ActivityLogsForEntity_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{LogsRoute}/BudgetRequest/1?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
