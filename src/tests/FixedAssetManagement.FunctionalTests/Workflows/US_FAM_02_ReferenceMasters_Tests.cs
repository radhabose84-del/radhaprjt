namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-02 — Reference master setup
//   As an asset administrator I set up the reference masters
//   (Location → SubLocation, UOM, Manufacture) used when onboarding assets.
// Fully implementable: all four are clean masters covered by the QA suite.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-02-ReferenceMasters")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_02_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string LocationRoute    = "/api/Location";
    private const string SubLocationRoute = "/api/SubLocation";
    private const string UomRoute         = "/api/fam/UOM";
    private const string ManufactureRoute = "/api/Manufacture";
    private const int Seed = 1;

    private static int _locationId;
    private static int _subLocationId;
    private static int _uomId;
    private static int _manufactureId;

    public US_FAM_02_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    // AC1 — create a Location.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateLocation()
    {
        var resp = await _f.Client.PostAsJsonAsync(LocationRoute, new
        {
            code = Code(), locationName = "QA FAM Location", description = "US-FAM-02",
            sortOrder = 1, unitId = Seed, departmentId = Seed
        });
        await QAHelper.AssertOkAsync(resp);
        _locationId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _locationId.Should().BeGreaterThan(0);
    }

    // AC2 — create a SubLocation under that Location.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateSubLocationUnderLocation()
    {
        _locationId.Should().BeGreaterThan(0, "Step1 must have created the location");
        var resp = await _f.Client.PostAsJsonAsync(SubLocationRoute, new
        {
            code = Code(), subLocationName = "QA FAM SubLocation", description = "US-FAM-02",
            unitId = Seed, departmentId = Seed, locationId = _locationId
        });
        await QAHelper.AssertOkAsync(resp);
        _subLocationId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _subLocationId.Should().BeGreaterThan(0);
    }

    // AC3 — create a UOM.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateUom()
    {
        var resp = await _f.Client.PostAsJsonAsync(UomRoute, new
        {
            code = Code(), uomName = "QA Unit", sortOrder = 1, uomTypeId = Seed
        });
        await QAHelper.AssertOkAsync(resp);
        _uomId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _uomId.Should().BeGreaterThan(0);
    }

    // AC4 — create a Manufacture (country/state/city FKs best-effort).
    [Fact, TestPriority(4)]
    public async Task Step4_CreateManufacture()
    {
        var cityId = _f.CityId > 0 ? _f.CityId : Seed;
        var manuType = await QAHelper.FirstIdAsync(_f.Client, $"{ManufactureRoute}/ManufactureType");
        var resp = await _f.Client.PostAsJsonAsync(ManufactureRoute, new
        {
            code = Code(), manufactureName = "QA Manufacturer", manufactureType = manuType > 0 ? manuType : Seed,
            countryId = Seed, stateId = Seed, cityId,
            addressLine1 = "QA Street", addressLine2 = "QA Area", pinCode = "600001",
            personName = "QA Person", phoneNumber = "9876543210", email = "qa@example.com"
        });
        await QAHelper.AssertOkAsync(resp);
        _manufactureId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _manufactureId.Should().BeGreaterThan(0);
    }

    // AC5 — each created master is readable by id.
    [Fact, TestPriority(5)]
    public async Task Step5_AllReadableById()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{LocationRoute}/{_locationId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{SubLocationRoute}/{_subLocationId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{UomRoute}/{_uomId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ManufactureRoute}/{_manufactureId}"));
    }

    // AC6 — teardown removes the created reference masters.
    [Fact, TestPriority(6)]
    public async Task Step6_Teardown()
    {
        if (_subLocationId > 0) await _f.Client.DeleteAsync($"{SubLocationRoute}/{_subLocationId}");
        if (_locationId > 0)    await _f.Client.DeleteAsync($"{LocationRoute}/{_locationId}");
        if (_uomId > 0)         await _f.Client.DeleteAsync($"{UomRoute}/{_uomId}");
        if (_manufactureId > 0) await _f.Client.DeleteAsync($"{ManufactureRoute}/{_manufactureId}");
    }
}
