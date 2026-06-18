namespace InventoryManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-INV-03 — Material flow & audit
//   As a stores user I requisition material (MRS), issue it against the approved MRS,
//   then review current stock and the audit trail.
// Blocked on the transactional create chain (needs Unit workflow config + warehouse stock);
// the read surfaces (MRS / Issue / CurrentStock) and the audit trail are reachable.
//
// Contracts (verified against InventoryManagement.QATests, 2026-06-17):
//   GET  /api/inventory/Mrs/MrsEntryDetails?pageNumber=&pageSize=
//   GET  /api/inventory/Issue/IssueEntryPendingHeaders?pageNumber=&pageSize=
//   GET  /api/Reports/CurrentStock                              (route is api/[controller])
//   GET  /api/inventory/AuditLog                                (Mongo-backed list)
//   POST /api/inventory/Mrs    /  POST /api/inventory/Issue     (nested transactional payloads — blocked)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-INV-03-MaterialFlowAudit")]
[Trait("Module", "InventoryManagement")]
[Trait("Story", "US-INV-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_INV_03_MaterialFlowAudit_Tests
{
    private readonly QAServerFixture _f;

    private const string MrsRoute      = "/api/inventory/Mrs";
    private const string IssueRoute    = "/api/inventory/Issue";
    private const string ReportsRoute  = "/api/Reports";
    private const string AuditLogRoute = "/api/inventory/AuditLog";

    public US_INV_03_MaterialFlowAudit_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the MRS / Issue / CurrentStock read surfaces are reachable (200/404 tolerant; empty stock valid).
    [Fact, TestPriority(1)]
    public async Task Step1_MaterialFlowReadSurfacesReachable()
    {
        var mrs = await _f.Client.GetAsync($"{MrsRoute}/MrsEntryDetails?pageNumber=1&pageSize=15");
        ((int)mrs.StatusCode).Should().BeOneOf(200, 404);

        var issue = await _f.Client.GetAsync($"{IssueRoute}/IssueEntryPendingHeaders?pageNumber=1&pageSize=15");
        ((int)issue.StatusCode).Should().BeOneOf(200, 404);

        var stock = await _f.Client.GetAsync($"{ReportsRoute}/CurrentStock");
        ((int)stock.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // AC2 — the MRS entry-details endpoint rejects anonymous callers (401).
    [Fact, TestPriority(2)]
    public async Task Step2_MrsEntryDetails_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{MrsRoute}/MrsEntryDetails?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // AC3 — an MRS can be raised and issued through to stock movement.
    // 🚫 blocked: needs the requesting Unit's workflow configuration + on-hand warehouse stock.
    [Fact(Skip = "needs seeded data: Unit workflow config + warehouse stock"), TestPriority(3)]
    public async Task Step3_RaiseMrs_Then_Issue()
    {
        // Raise an MRS for a real active item.
        var mrsResp = await _f.Client.PostAsJsonAsync(MrsRoute, new
        {
            warehouseId = 1,
            mrsDetails = new[]
            {
                new { itemId = _f.ActiveItemId, quantity = 1m }
            }
        });
        await QAHelper.AssertOkAsync(mrsResp);
        var mrsId = (await QAHelper.ParseAsync(mrsResp)).RootElement.CreatedId();
        mrsId.Should().BeGreaterThan(0);

        // Issue against the approved MRS.
        var issueResp = await _f.Client.PostAsJsonAsync(IssueRoute, new
        {
            mrsId,
            issueDetails = new[]
            {
                new { itemId = _f.ActiveItemId, quantity = 1m, warehouseId = 1 }
            }
        });
        await QAHelper.AssertOkAsync(issueResp);
    }

    // AC4 — the inventory audit trail can be listed (Mongo-backed; empty set valid → 200/404 tolerant).
    [Fact, TestPriority(4)]
    public async Task Step4_AuditTrailIsListable()
    {
        var resp = await _f.Client.GetAsync(AuditLogRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
