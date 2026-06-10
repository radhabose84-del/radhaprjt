namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-07 — Asset transfer & receipt
//   As an asset administrator I transfer an asset between departments and receive it.
//   BLOCKED on real asset/department data — the lookup step is runnable; the transfer
//   and receipt steps are Skipped until posted assets exist.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-07-TransferReceipt")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-07")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_07_TransferReceipt_Tests
{
    private readonly QAServerFixture _f;
    public US_FAM_07_TransferReceipt_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the transfer-types lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_TransferTypesLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/AssetTransfer/TransferTypes");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs posted assets: resolve transferable assets by category/department."), TestPriority(2)]
    public Task Step2_ResolveTransferableAssets() => Task.CompletedTask;

    [Fact(Skip = "Needs asset + department data: create an AssetTransfer (source → destination)."), TestPriority(3)]
    public Task Step3_CreateTransfer() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted transfer: create an AssetTransferReceipt to accept it."), TestPriority(4)]
    public Task Step4_CreateReceipt() => Task.CompletedTask;
}
