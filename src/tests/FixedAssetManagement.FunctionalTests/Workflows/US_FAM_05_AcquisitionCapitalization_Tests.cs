namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-05 — Asset acquisition & capitalization
//
//   As an asset administrator I record an asset's purchase against a GRN and add
//   capitalized costs.
//
// Live-reconciled status:
//   • AssetSource lookup is reachable (Step1).
//   • AssetAdditionalCost can be added against a built asset (Step3).
//   • BLOCKED: AssetPurchase create is GRN-driven — it links a GRN (GetGrnNo/GetGrnItems by
//     OldUnitId + AssetSourceId) to the asset. testsales has no OldUnitId GRN scope, so there
//     is no GRN to purchase against. Capitalization (Step4) depends on that posted purchase.
//     Needs a unit-scoped QA user with seeded GRN data.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-05-AcquisitionCapitalization")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-05")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_05_AcquisitionCapitalization_Tests
{
    private readonly QAServerFixture _f;
    private const string PurchaseRoute = "/api/AssetPurchase";
    private const string AdditionalCostRoute = "/api/assetadditionalcost";
    private const int Seed = 1;

    public US_FAM_05_AcquisitionCapitalization_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the AssetSource lookup is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_AssetSourceLookupReachable()
    {
        var resp = await _f.Client.GetAsync($"{PurchaseRoute}/AssetSource/by-name?SourceName=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // AC2 — record the purchase against a GRN.
    [Fact(Skip = "Blocked (environment): AssetPurchase create is GRN-driven (GetGrnNo/GetGrnItems by OldUnitId + AssetSourceId). testsales has no OldUnitId GRN scope, so there is no GRN to link. Needs a unit-scoped QA user with seeded GRN data."), TestPriority(2)]
    public Task Step2_CreateAssetPurchaseFromGrn() => Task.CompletedTask;

    // AC3 — add an AssetAdditionalCost against the asset (capitalized cost).
    [Fact, TestPriority(3)]
    public async Task Step3_AddAdditionalCost()
    {
        var assetId = await FamAssetBuilder.BuildAssetAsync(_f.Client, _f.EntityCode);
        var assetSourceId = await QAHelper.FirstIdAsync(_f.Client, $"{PurchaseRoute}/AssetSource/by-name?SourceName=QA");

        var resp = await _f.Client.PostAsJsonAsync(AdditionalCostRoute, new
        {
            assetId,
            assetSourceId = assetSourceId > 0 ? assetSourceId : Seed,
            amount = 500.0,
            journalNo = "JN" + _f.EntityCode[..6],
            costType = Seed
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
    }

    // AC4 — the asset shows a capitalization date after the purchase is posted.
    [Fact(Skip = "Needs a posted AssetPurchase (blocked by Step2 — GRN/OldUnitId)."), TestPriority(4)]
    public Task Step4_AssetCapitalized() => Task.CompletedTask;
}
