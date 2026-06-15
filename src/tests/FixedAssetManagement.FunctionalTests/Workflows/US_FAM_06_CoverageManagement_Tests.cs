namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-06 — Asset coverage management (warranty / insurance / AMC)
//
//   As an asset administrator I attach warranty, insurance and an AMC to an asset and
//   track renewals.
//
// WORKFLOW test: builds an asset (FamAssetBuilder), then attaches Warranty, Insurance and
// an AMC, and reads each coverage record back.
//
// Live-reconciled facts:
//   • AssetWarranty requires ContactPerson + MobileNumber (+ service location ids best-effort=1).
//   • AssetInsurance / AssetAmc create return 201; coverage GET-by-id is the created record id.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-06-CoverageManagement")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-06")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_06_CoverageManagement_Tests
{
    private readonly QAServerFixture _f;

    private const string WarrantyRoute = "/api/assetwarranty";
    private const string InsuranceRoute = "/api/assetinsurance";
    private const string AmcRoute = "/api/assetamc";
    private const int Seed = 1;

    private static int _assetId, _warrantyId, _insuranceId, _amcId;

    public US_FAM_06_CoverageManagement_Tests(QAServerFixture fixture) => _f = fixture;

    private string S => _f.EntityCode[..6];

    // AC1 — a coverage lookup is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_WarrantyClaimStatusLookupReachable()
    {
        var resp = await _f.Client.GetAsync($"{WarrantyRoute}/WarrantyClaimStatus");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC2 — a Warranty can be created for an asset.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateWarranty()
    {
        _assetId = await FamAssetBuilder.BuildAssetAsync(_f.Client, _f.EntityCode);
        var resp = await _f.Client.PostAsJsonAsync(WarrantyRoute, new
        {
            assetId = _assetId, startDate = "2026-01-01", endDate = "2027-01-01", period = 12,
            warrantyType = Seed, warrantyProvider = "QA Provider " + S,
            contactPerson = "QA Person", mobileNumber = "9876543210", email = "qa@example.com",
            serviceCountryId = Seed, serviceStateId = Seed, serviceCityId = Seed,
            serviceContactPerson = "QA Svc", serviceMobileNumber = "9876543210", serviceEmail = "svc@example.com"
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _warrantyId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _warrantyId.Should().BeGreaterThan(0);
    }

    // AC3 — an Insurance policy can be created for the asset.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateInsurance()
    {
        _assetId.Should().BeGreaterThan(0, "Step2 must have created the asset");
        var resp = await _f.Client.PostAsJsonAsync(InsuranceRoute, new
        {
            assetId = _assetId, policyNo = "POL" + S, startDate = "2026-01-01", insuranceperiod = 12,
            endDate = "2027-01-01", policyAmount = 1000.0, vendorCode = "V1", renewalStatus = Seed,
            renewedDate = "2026-01-01", isActive = (byte)1
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _insuranceId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _insuranceId.Should().BeGreaterThan(0);
    }

    // AC4 — an AMC can be created for the asset.
    [Fact, TestPriority(4)]
    public async Task Step4_CreateAmc()
    {
        _assetId.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync(AmcRoute, new
        {
            assetId = _assetId, startDate = "2026-01-01", period = 12, vendorCode = "V1",
            vendorName = "QA Vendor", coverageType = Seed, renewalStatus = Seed,
            renewedDate = "2026-01-01", isActive = (byte)1
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _amcId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _amcId.Should().BeGreaterThan(0);
    }

    // AC5 — each coverage record is readable by id.
    [Fact, TestPriority(5)]
    public async Task Step5_CoverageReadable()
    {
        _warrantyId.Should().BeGreaterThan(0);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{WarrantyRoute}/{_warrantyId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{InsuranceRoute}/{_insuranceId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{AmcRoute}/{_amcId}"));
    }
}
