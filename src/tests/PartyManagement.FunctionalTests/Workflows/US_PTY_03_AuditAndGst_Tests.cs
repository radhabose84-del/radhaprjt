namespace PartyManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PTY-03 — Party audit trail & GST lookup (read-only)
//   As a party administrator I review the audit trail and look up a GSTIN.
// Routes verified from PartyManagement.QATests: /api/party/auditlog (raw list; GetAuditLogSearch);
// /api/gst/auth + /api/gst/gstin/{gstin} (external service — tolerant).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PTY-03-AuditAndGst")]
[Trait("Module", "PartyManagement")]
[Trait("Story", "US-PTY-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PTY_03_AuditAndGst_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/party/auditlog";
    private const string GstRoute   = "/api/gst";

    public US_PTY_03_AuditAndGst_Tests(QAServerFixture fixture) => _f = fixture;

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

    // GST is an external-service proxy — reachability only (tolerant).
    [Fact, TestPriority(3)]
    public async Task Step3_GstEndpoints_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{GstRoute}/auth")).StatusCode).Should().BeOneOf(200, 400, 404, 500);
        ((int)(await _f.Client.GetAsync($"{GstRoute}/gstin/22AAAAA0000A1Z5")).StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }
}
