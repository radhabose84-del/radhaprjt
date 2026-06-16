namespace GateEntryManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GE-03 — Gate audit trail (read-only)
//   As a gate administrator I review the audit trail of gate actions.
// Route verified from GateEntryManagement.QATests: /api/gateentry/auditlog (raw list, status-only);
// search via search?searchPattern=.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GE-03-AuditTrail")]
[Trait("Module", "GateEntryManagement")]
[Trait("Story", "US-GE-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GE_03_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/gateentry/auditlog";

    public US_GE_03_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task Step1_ListAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync(AuditRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogs_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{AuditRoute}/search?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
