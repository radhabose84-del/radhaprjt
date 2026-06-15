namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-05 — Asset acquisition & capitalization
//   As an asset administrator I record an asset's purchase against a GRN and add
//   capitalized costs. BLOCKED on real GRN data — the lookup step is runnable; the
//   GRN-driven create steps are Skipped until a QA GRN data set exists.
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
    public US_FAM_05_AcquisitionCapitalization_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the AssetSource lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_AssetSourceLookupReachable()
    {
        var resp = await _f.Client.GetAsync($"{PurchaseRoute}/AssetSource/by-name?SourceName=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact(Skip = "Needs real GRN data: create AssetPurchase linking GRN → Asset with PurchaseValue + CapitalizationDate."), TestPriority(2)]
    public Task Step2_CreateAssetPurchaseFromGrn() => Task.CompletedTask;

    [Fact(Skip = "Needs the created asset id: add an AssetAdditionalCost against the asset."), TestPriority(3)]
    public Task Step3_AddAdditionalCost() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted purchase: verify the asset shows a capitalization date."), TestPriority(4)]
    public Task Step4_AssetCapitalized() => Task.CompletedTask;
}
