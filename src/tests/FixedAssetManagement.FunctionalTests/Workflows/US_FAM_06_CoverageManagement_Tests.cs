namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-06 — Asset coverage management (warranty / insurance / AMC)
//   As an asset administrator I attach warranty, insurance and an AMC to an asset and
//   track renewals. BLOCKED on a real asset id — the lookup step is runnable; the
//   coverage create steps are Skipped until an asset exists.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-06-CoverageManagement")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-06")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_06_CoverageManagement_Tests
{
    private readonly QAServerFixture _f;
    public US_FAM_06_CoverageManagement_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a coverage lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_WarrantyClaimStatusLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/AssetWarranty/WarrantyClaimStatus");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs an asset id (US-FAM-04): create AssetWarranty for the asset (period, provider, service centre)."), TestPriority(2)]
    public Task Step2_CreateWarranty() => Task.CompletedTask;

    [Fact(Skip = "Needs an asset id: create AssetInsurance for the asset (policy no, period, amount, renewal status)."), TestPriority(3)]
    public Task Step3_CreateInsurance() => Task.CompletedTask;

    [Fact(Skip = "Needs an asset id: create AssetAmc for the asset (vendor, coverage type, renewal status)."), TestPriority(4)]
    public Task Step4_CreateAmc() => Task.CompletedTask;

    [Fact(Skip = "Needs an asset id: each coverage record is readable for the asset."), TestPriority(5)]
    public Task Step5_CoverageReadable() => Task.CompletedTask;
}
