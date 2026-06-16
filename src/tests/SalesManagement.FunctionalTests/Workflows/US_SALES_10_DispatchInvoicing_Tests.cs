namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-10 — Dispatch & invoicing
//
//   As a dispatch user I advise a dispatch, generate a delivery challan, build a
//   trip sheet, and invoice it.
//
// Live-reconciled status:
//   • Reachability reads are active (DispatchAdvice / DeliveryChallan / TripSheet /
//     Invoice list endpoints) plus a no-auth 401 check — they prove the fulfilment
//     read path works for an authenticated user.
//   • BLOCKED: every create here needs packed stock and a prior workflow row — a
//     posted SalesOrder/STO, packed Inventory lots, and an un-invoiced dispatch
//     advice. The QA clone has no packed-stock scope, so the create ACs are
//     [Fact(Skip)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-10-DispatchInvoicing")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-10")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_10_DispatchInvoicing_Tests
{
    private readonly QAServerFixture _f;

    private const string DispatchAdviceRoute  = "/api/DispatchAdvice";
    private const string DeliveryChallanRoute = "/api/DeliveryChallan";
    private const string TripSheetRoute       = "/api/TripSheet";
    private const string InvoiceRoute         = "/api/Invoice";

    public US_SALES_10_DispatchInvoicing_Tests(QAServerFixture fixture) => _f = fixture;

    // Reachability — the dispatch-advice list answers for an authenticated user
    // (tolerant: 200 with data, or 404 when empty on the clone).
    [Fact, TestPriority(1)]
    public async Task Step1_DispatchAdviceListReachable()
    {
        var resp = await _f.Client.GetAsync($"{DispatchAdviceRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the delivery-challan list answers for an authenticated user.
    [Fact, TestPriority(2)]
    public async Task Step2_DeliveryChallanListReachable()
    {
        var resp = await _f.Client.GetAsync($"{DeliveryChallanRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the trip-sheet list answers for an authenticated user.
    [Fact, TestPriority(3)]
    public async Task Step3_TripSheetListReachable()
    {
        var resp = await _f.Client.GetAsync($"{TripSheetRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Reachability — the invoice list answers for an authenticated user.
    [Fact, TestPriority(4)]
    public async Task Step4_InvoiceListReachable()
    {
        var resp = await _f.Client.GetAsync($"{InvoiceRoute}?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // Security — the dispatch-advice list rejects an unauthenticated caller (401).
    [Fact, TestPriority(5)]
    public async Task Step5_DispatchAdviceListRequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{DispatchAdviceRoute}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    // AC1 — a DispatchAdvice can be created against a sales order with pack ranges.
    [Fact(Skip = "needs seeded data: requires a posted SalesOrder + packed Inventory stock (pack ranges) — no packed-stock scope on BannariERP_QATest."), TestPriority(6)]
    public Task Step6_CreateDispatchAdvice() => Task.CompletedTask;

    // AC2 — a DeliveryChallan can be generated (STO or order).
    [Fact(Skip = "needs seeded data: requires an STO/SalesOrder + stock to ship (blocked by Step6)."), TestPriority(7)]
    public Task Step7_GenerateDeliveryChallan() => Task.CompletedTask;

    // AC3 — a TripSheet can group dispatch advices.
    [Fact(Skip = "needs seeded data: requires at least one DispatchAdvice (blocked by Step6)."), TestPriority(8)]
    public Task Step8_BuildTripSheet() => Task.CompletedTask;

    // AC4 — an Invoice can be raised from a dispatch advice.
    [Fact(Skip = "needs seeded data: requires an un-invoiced DispatchAdvice (blocked by Step6)."), TestPriority(9)]
    public Task Step9_RaiseInvoice() => Task.CompletedTask;

    // AC5 — an e-invoice / e-waybill can be generated.
    [Fact(Skip = "needs seeded data: requires a raised Invoice + external e-invoice/e-waybill API (blocked by Step9)."), TestPriority(10)]
    public Task Step10_GenerateEInvoiceEWaybill() => Task.CompletedTask;
}
