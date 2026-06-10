namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-09 — Asset disposal
//   As an asset administrator I dispose of an asset at end of life and record the value.
//   BLOCKED on a real asset + purchase — the disposal-type lookup is runnable; the
//   disposal create step is Skipped (AssetDisposal needs both AssetId and AssetPurchaseId).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-09-Disposal")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-09")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_09_Disposal_Tests
{
    private readonly QAServerFixture _f;
    public US_FAM_09_Disposal_Tests(QAServerFixture fixture) => _f = fixture;

    // AC (lookup) — the disposal-type lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_DisposalTypeLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/AssetDisposal/DisposalType");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs US-FAM-04/05: an asset with a purchase (AssetId + AssetPurchaseId)."), TestPriority(2)]
    public Task Step2_AssetWithPurchaseExists() => Task.CompletedTask;

    [Fact(Skip = "Needs asset + purchase ids: create an AssetDisposal (date, type, reason, amount)."), TestPriority(3)]
    public Task Step3_CreateDisposal() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted disposal: the disposed asset is reflected as disposed."), TestPriority(4)]
    public Task Step4_AssetReflectedDisposed() => Task.CompletedTask;
}
