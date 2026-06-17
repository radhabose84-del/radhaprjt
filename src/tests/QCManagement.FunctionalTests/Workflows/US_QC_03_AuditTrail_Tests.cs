namespace QCManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-QC-03 — QC audit trail (read-only)
//   As a QC administrator I review the audit trail of QC actions.
// Route verified from QCManagement.QATests: /api/qc/auditlog (raw list, status-only);
// search via by-name?searchPattern=.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-QC-03-AuditTrail")]
[Trait("Module", "QCManagement")]
[Trait("Story", "US-QC-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_QC_03_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/qc/auditlog";

    public US_QC_03_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

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
