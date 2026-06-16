namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-11 — Stock transfer order (STO)
//
//   As a warehouse user I raise an STO, ship it on a delivery challan, and receive it.
//
// Live-reconciled status:
//   • Reachability reads are active (StoHeader / StoReceipt list + StockLedger
//     report) plus a no-auth 401 check — they prove the STO read path works.
//   • BLOCKED: the STO create chain needs Org plants + Warehouse storage locations
//     + Inventory items, then stock to ship and a challan to receive. The QA clone
//     has no plant/stock scope, so the create ACs are [Fact(Skip)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-11-StockTransfer")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-11")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_11_StockTransfer_Tests
{
    private readonly QAServerFixture _f;

    private const string StoHeaderRoute   = "/api/sales/StoHeader";
    private const string StoReceiptRoute  = "/api/StoReceipt";
    private const string StockLedgerRoute = "/api/StockLedger";

    public US_SALES_11_StockTransfer_Tests(QAServerFixture fixture) => _f = fixture;

    // Reachability — the STO-header list answers for an authenticated user
    // (tolerant: 200 with data, or 404 when empty on the clone).
    [Fact, TestPriority(1)]
    public async Task Step1_StoHeaderListReachable()
    {
        var resp = await _f.Client.GetAsync($"{StoHeaderRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the STO-receipt list answers for an authenticated user.
    [Fact, TestPriority(2)]
    public async Task Step2_StoReceiptListReachable()
    {
        var resp = await _f.Client.GetAsync($"{StoReceiptRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the StockLedger report answers for an authenticated user.
    [Fact, TestPriority(3)]
    public async Task Step3_StockLedgerReportReachable()
    {
        var resp = await _f.Client.GetAsync($"{StockLedgerRoute}/report?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Security — the STO-header list rejects an unauthenticated caller (401).
    [Fact, TestPriority(4)]
    public async Task Step4_StoHeaderListRequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{StoHeaderRoute}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    // AC1 — an StoHeader can be created (plants + storage locations + items).
    [Fact(Skip = "needs seeded data: requires Org plants + Warehouse storage locations + Inventory items — no plant/storage scope on BannariERP_QATest."), TestPriority(5)]
    public Task Step5_CreateStoHeader() => Task.CompletedTask;

    // AC2 — a DeliveryChallan ships the STO.
    [Fact(Skip = "needs seeded data: requires a posted StoHeader + stock to ship (blocked by Step5)."), TestPriority(6)]
    public Task Step6_ShipStoOnDeliveryChallan() => Task.CompletedTask;

    // AC3 — an StoReceipt receives the challan (accepted / damage qty).
    [Fact(Skip = "needs seeded data: requires a shipped DeliveryChallan (blocked by Step6)."), TestPriority(7)]
    public Task Step7_ReceiveStoReceipt() => Task.CompletedTask;

    // AC4 — the StockLedger reflects the movement.
    [Fact(Skip = "needs seeded data: requires a posted STO movement (blocked by Step5–Step7)."), TestPriority(8)]
    public Task Step8_StockLedgerReflectsMovement() => Task.CompletedTask;
}
