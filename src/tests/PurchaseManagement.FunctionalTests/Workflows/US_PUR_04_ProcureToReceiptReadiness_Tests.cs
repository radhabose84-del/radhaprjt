namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-04 — Procure-to-receipt readiness  (MOSTLY BLOCKED — readiness/reachability)
//   As a procurement user I expect the procure-to-receipt pipeline endpoints
//   (indent → PO → GRN) to be reachable and secured even before seeded
//   transactional data exists.
// The full PO document flow is [Skip] pending seeded data; the read surfaces are
// reachable (200/404 tolerant) and auth-protected.
//
// Contracts (verified against PurchaseManagement.QATests, 2026-06-17):
//   GET    /api/PurchaseIndent/pending?PageNumber=&PageSize=
//   GET    /api/PurchaseOrderLocal/pending-po?pageNumber=&pageSize=
//   GET    /api/GRNEntry/GRNEntryPendingHeader?PageNumber=&PageSize=
//   (full PO create chain blocked — needs vendor/item/paymentTerm + budget + doc-numbering seed)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-04-ProcureToReceiptReadiness")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-04")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_04_ProcureToReceiptReadiness_Tests
{
    private readonly QAServerFixture _f;

    private const string IndentPendingRoute = "/api/PurchaseIndent/pending?PageNumber=1&PageSize=5";
    private const string PoPendingRoute     = "/api/PurchaseOrderLocal/pending-po?pageNumber=1&pageSize=5";
    private const string GrnPendingRoute    = "/api/GRNEntry/GRNEntryPendingHeader?pageNumber=1&pageSize=5";

    public US_PUR_04_ProcureToReceiptReadiness_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a full Purchase Order can be created end-to-end.
    // 🚫 Blocked: needs vendor/item/paymentTerm + budget + doc-numbering "Purchase Order" seed.
    // Documentary placeholder — un-skip once a unit-scoped QA user with the seeded config exists.
    [Fact(Skip = "needs seeded data: vendor/item/paymentTerm + budget + doc-numbering 'Purchase Order'"), TestPriority(1)]
    public Task Step1_FullPurchaseOrderCreate_EndToEnd()
    {
        // Intentionally empty — the PO document flow requires seeded transactional masters.
        return Task.CompletedTask;
    }

    // AC2 — the PurchaseIndent pending list is reachable (tolerant 200/404).
    [Fact, TestPriority(2)]
    public async Task Step2_PurchaseIndentPendingReachable()
    {
        var resp = await _f.Client.GetAsync(IndentPendingRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC3 — the PurchaseOrderLocal pending-PO list is reachable (tolerant 200/404).
    [Fact, TestPriority(3)]
    public async Task Step3_PurchaseOrderLocalPendingReachable()
    {
        var resp = await _f.Client.GetAsync(PoPendingRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC4 — the GRNEntry pending-header list is reachable (tolerant 200/404).
    [Fact, TestPriority(4)]
    public async Task Step4_GrnEntryPendingHeaderReachable()
    {
        var resp = await _f.Client.GetAsync(GrnPendingRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC5 — each pipeline read rejects anonymous callers (401). Proves the chain is auth-protected.
    [Fact, TestPriority(5)]
    public async Task Step5_PipelineReadsRejectAnonymous()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(IndentPendingRoute));
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(PoPendingRoute));
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(GrnPendingRoute));
    }
}
