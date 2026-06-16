namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-12 — Fixed-asset audit log query (read-only)
//
//   As a fixed-asset administrator I review the audit trail of asset actions, listing
//   all entries and searching them by a pattern.
//
// READ-ONLY story: verifies the audit log read contract (list + search) is reachable.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-12-AuditLog")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-12")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_12_AuditLog_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditLogRoute = "/api/fam/AuditLog";

    public US_FAM_12_AuditLog_Tests(QAServerFixture fixture) => _f = fixture;

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
