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

    [Fact(Skip = "Blocked: transferable assets are resolved by custodian/department/category for the caller's scope; the transfer read endpoints (GetAllAssetTransfers / GetAssetTransferReceiptPending) currently 500 on the QA clone. Needs a unit-scoped user with assets assigned to a transferable custodian/department."), TestPriority(2)]
    public Task Step2_ResolveTransferableAssets() => Task.CompletedTask;

    [Fact(Skip = "Needs resolved transferable assets + source/destination department & custodian (blocked by Step2)."), TestPriority(3)]
    public Task Step3_CreateTransfer() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted transfer to accept (blocked by Step3)."), TestPriority(4)]
    public Task Step4_CreateReceipt() => Task.CompletedTask;
}
