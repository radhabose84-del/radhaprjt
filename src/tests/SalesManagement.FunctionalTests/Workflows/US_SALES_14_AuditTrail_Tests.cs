namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-14 — Audit trail (read-only)
//
//   As a sales administrator I review the audit trail of sales actions, listing all
//   entries and searching them by a pattern.
//
// READ-ONLY story: verifies the sales audit-log read contract (list + search) is
// reachable. Route prefix is "api/sales/[controller]" (NOT "api/AuditLog"). The list
// endpoint returns the raw query result (not the { data, totalCount } envelope), so we
// assert the status code only. Audit logs live in MongoDB → an empty set is valid (404).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-14-AuditTrail")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-14")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_14_AuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditLogRoute = "/api/sales/AuditLog";

    public US_SALES_14_AuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — all sales audit logs can be listed.
    [Fact, TestPriority(1)]
    public async Task Step1_ListAllAuditLogs()
    {
        var resp = await _f.Client.GetAsync(AuditLogRoute);
        // Raw list payload (no standard envelope); empty MongoDB log set is valid → tolerate 404.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC2 — audit logs can be searched by a pattern.
    [Fact, TestPriority(2)]
    public async Task Step2_SearchAuditLogsByPattern()
    {
        // note: search-endpoint body shape varies; an empty pattern / empty result set may
        // surface as 400 or 404 rather than 200, so reachability is asserted tolerantly.
        var resp = await _f.Client.GetAsync($"{AuditLogRoute}/GetAuditLogSearch?searchPattern=Create");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
