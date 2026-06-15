namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-12 — Maintenance audit log query (read-only)
//
//   As a maintenance administrator I review the audit trail of maintenance actions,
//   listing all entries and searching them by a pattern.
//
// READ-ONLY story: the audit log is written as a side effect of other actions, so this
// story only verifies the read contract (list + search) is reachable and returns 200.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-12-AuditLog")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-12")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_12_AuditLog_Tests
{
    private readonly QAServerFixture _f;

    private const string AuditLogRoute = "/api/maintenance/AuditLog";

    public US_MNT_12_AuditLog_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — all audit logs can be listed.
    [Fact, TestPriority(1)]
    public async Task Step1_ListAllAuditLogs()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync(AuditLogRoute));
    }

    // AC2 — audit logs can be searched by a pattern.
    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogs()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{AuditLogRoute}/GetAuditLogSearch?searchPattern=QA"));
    }
}
