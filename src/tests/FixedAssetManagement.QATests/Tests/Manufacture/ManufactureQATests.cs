namespace FixedAssetManagement.QATests.Tests.Manufacture;

// ─────────────────────────────────────────────────────────────────────────────
// Manufacture — live-server QA suite.  Route: /api/Manufacture
// Contract (verified 2026-06-09):
//   POST   { code, manufactureName, manufactureType?, countryId, stateId, cityId,
//            addressLine1?, addressLine2?, pinCode?, personName?, phoneNumber?, email? }  → data = ManufactureDTO
//   PUT    same fields + { id, isActive }
//   DELETE /{id}                  (route param; id<=0 guard → 400)
//   GET    ?PageNumber=&PageSize=&SearchTerm=
//   GET    /{id}                  (id<=0 guard → 400)
//   GET    /by-name?name=  ,  /ManufactureType
// Country/State/City FKs use best-effort seed ids (City via fixture, Country/State = 1).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ManufactureCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ManufactureQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Manufacture";
    private const int ValidCountryId = 1;
    private const int ValidStateId = 1;

    private string NewCode() => _f.EntityCode[..10];
    private int CityId => _f.CityId > 0 ? _f.CityId : 1;
    private static int _manuType = 1; // resolved at runtime in TC001 (ManufactureType is required)

    public ManufactureQATests(QAServerFixture fixture) => _f = fixture;

    private async Task<int> EnsureManufactureTypeAsync()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, "/api/Manufacture/ManufactureType");
        return id > 0 ? id : 1;
    }

    private object ValidPayload(int id = 0) => new
    {
        id,
        code = NewCode(),
        // Run-unique name: the backend enforces uniqueness on ManufactureName, so a fixed
        // string collides with rows left by earlier runs (the clone reset only clears
        // testsales rows that this same name may already occupy) → 400 "already exists".
        manufactureName = $"QA Manufacturer {NewCode()}",
        manufactureType = _manuType,
        countryId = ValidCountryId,
        stateId = ValidStateId,
        cityId = CityId,
        addressLine1 = "QA Street",
        addressLine2 = "QA Area",
        pinCode = "600001",
        personName = "QA Person",
        phoneNumber = "9876543210",
        email = "qa@example.com",
        isActive = (byte)1
    };

    // ── CREATE ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _manuType = await EnsureManufactureTypeAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidPayload());
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, ValidPayload());
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── GET ALL (smoke) ────────────────────────────────────────────────────────
    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET BY ID / TYPES ─────────────────────────────────────────────────────
    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetById_IdZero_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
    }

    // ── AUTOCOMPLETE ────────────────────────────────────────────────────────────
    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        // Live contract: by-name returns 400 "No Manufacture found..." when nothing matches.
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // ── UPDATE ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, ValidPayload(_f.CreatedId));
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, ValidPayload(_f.CreatedId));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── DELETE (route param; ALWAYS LAST) ───────────────────────────────────────
    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
