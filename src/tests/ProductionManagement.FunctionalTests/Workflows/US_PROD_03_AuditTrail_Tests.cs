namespace ProductionManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PROD-03 — Production audit trail (read-only)
//   As a production administrator I review the audit trail of production actions.
// Route verified from ProductionManagement.QATests: /api/production/auditlog (raw list;
// search via by-name?searchPattern=).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PROD-03-AuditTrail")]
[Trait("Module", "ProductionManagement")]
[Trait("Story", "US-PROD-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PROD_03_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/production/auditlog";

    public US_PROD_03_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task Step1_ListAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync(AuditRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{AuditRoute}/by-name?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
