namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-09 — Sales order lifecycle
//
//   As a sales executive I create a sales order, raise a proforma invoice, and
//   amend/cancel/foreclose it.
//
// Live-reconciled status:
//   • Reachability reads are active (SalesOrder list + pending list) — they prove
//     login → auth → DB → read works for the order endpoints.
//   • BLOCKED: the SalesOrder create chain needs cross-module seeded data the QA
//     clone does not guarantee — Party customers/agents, Inventory items/HSN, a
//     sales agreement + discount/commission chain, then the proforma/amendment
//     documents that build on the posted order. Those create ACs are [Fact(Skip)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-09-OrderLifecycle")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-09")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_09_OrderLifecycle_Tests
{
    private readonly QAServerFixture _f;

    private const string SalesOrderRoute = "/api/SalesOrder";

    public US_SALES_09_OrderLifecycle_Tests(QAServerFixture fixture) => _f = fixture;

    // Reachability — the SalesOrder list endpoint answers for an authenticated user
    // (tolerant: 200 with data, or 404 when the order index is empty on the clone).
    [Fact, TestPriority(1)]
    public async Task Step1_SalesOrderListReachable()
    {
        var resp = await _f.Client.GetAsync($"{SalesOrderRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the pending-orders list answers for an authenticated user.
    [Fact, TestPriority(2)]
    public async Task Step2_PendingOrdersReachable()
    {
        var resp = await _f.Client.GetAsync($"{SalesOrderRoute}/pending?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Security — the SalesOrder list rejects an unauthenticated caller (401).
    [Fact, TestPriority(3)]
    public async Task Step3_SalesOrderListRequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{SalesOrderRoute}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    // AC1 — a SalesOrder can be created (party + items + discounts + commission).
    [Fact(Skip = "needs seeded data: SalesOrder create is a large nested DTO requiring a Party customer/agent, Inventory items + HSN, a SalesAgreement and the discount/commission chain — none guaranteed on BannariERP_QATest."), TestPriority(4)]
    public Task Step4_CreateSalesOrder() => Task.CompletedTask;

    // AC2 — a ProformaInvoice can be raised against the order.
    [Fact(Skip = "needs seeded data: requires a posted SalesOrder (blocked by Step4)."), TestPriority(5)]
    public Task Step5_RaiseProformaInvoice() => Task.CompletedTask;

    // AC3 — proforma payment can be recorded (update-payment).
    [Fact(Skip = "needs seeded data: requires a raised ProformaInvoice (blocked by Step5)."), TestPriority(6)]
    public Task Step6_RecordProformaPayment() => Task.CompletedTask;

    // AC4 — a SalesOrderAmendment can amend the order.
    [Fact(Skip = "needs seeded data: requires a posted SalesOrder to amend (blocked by Step4)."), TestPriority(7)]
    public Task Step7_AmendSalesOrder() => Task.CompletedTask;

    // AC5 — the order can be cancelled / foreclosed.
    [Fact(Skip = "needs seeded data: requires a posted SalesOrder to cancel/foreclose (blocked by Step4)."), TestPriority(8)]
    public Task Step8_CancelOrForecloseOrder() => Task.CompletedTask;
}
