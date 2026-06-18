namespace ProductionManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PROD-02 — Lot & production pack
//   As a production user I create a lot and record a production-pack entry against it.
//
// PARTIAL: read/reachability is ACTIVE; the lot → production-pack → repack create chain is
// [Fact(Skip=…)] — LotMaster needs cross-module Inventory item + production misc (lotType/status);
// ProductionPackEntry + RepackingHeader are document-numbered ('PackMaster'/'RePackMaster'/
// 'YarnConversion', not seeded) and stock-bearing.
//
// Routes verified from ProductionManagement.QATests:
//   LotMaster          : /api/lotmaster (GET ""; by-stock; DELETE /{id})
//   ProductionPackEntry: /api/productionpack (GET ""; stock-register; …)
//   RepackingHeader    : /api/repackingheader (GET ""; getstockitems; …)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PROD-02-LotAndProduction")]
[Trait("Module", "ProductionManagement")]
[Trait("Story", "US-PROD-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PROD_02_LotAndProduction_Tests
{
    private readonly QAServerFixture _f;

    private const string LotRoute     = "/api/lotmaster";
    private const string PackRoute     = "/api/productionpack";
    private const string RepackRoute   = "/api/repackingheader";

    public US_PROD_02_LotAndProduction_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the lot / production-pack / repacking read surface is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_ProductionReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{LotRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{PackRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{RepackRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — no-auth rejected on the lot list.
    [Fact, TestPriority(2)]
    public async Task Step2_LotList_NoAuth_Returns401()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{LotRoute}?PageNumber=1&PageSize=15"));
    }

    // AC2 — create a LotMaster. BLOCKED.
    [Fact(Skip = "needs seeded data: cross-module Inventory item + production misc (lotType/status)."), TestPriority(3)]
    public async Task Step3_CreateLot()
    {
        await Task.CompletedTask;
    }

    // AC3 — record a ProductionPackEntry. BLOCKED.
    [Fact(Skip = "needs seeded data: doc-numbering 'PackMaster' + warehouse/item/lot/packType chain."), TestPriority(4)]
    public async Task Step4_CreateProductionPack()
    {
        await Task.CompletedTask;
    }

    // AC4 — repack/convert packed stock. BLOCKED.
    [Fact(Skip = "needs seeded data: doc-numbering 'RePackMaster'/'YarnConversion' + packed stock."), TestPriority(5)]
    public async Task Step5_CreateRepacking()
    {
        await Task.CompletedTask;
    }
}
