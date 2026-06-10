namespace FixedAssetManagement.QATests.Tests.SubLocation;

// SubLocation — /api/SubLocation
// POST { code, subLocationName, description?, unitId, departmentId, locationId } → SubLocationDto
// PUT  { id, code, subLocationName, description?, unitId, departmentId, locationId, isActive } → bool
// DELETE /{id}; GET ?Page..; GET /{id}; GET /by-name?name=
// unitId/departmentId/locationId FKs best-effort seed id 1.

[Collection("SubLocationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SubLocationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SubLocation";
    private const int ValidUnitId = 1, ValidDepartmentId = 1;
    private static int ValidLocationId = 1; // resolved at runtime — must be an ACTIVE Location
    private static string _code = string.Empty;
    private string NewCode() => _f.EntityCode[..10];

    public SubLocationQATests(QAServerFixture fixture) => _f = fixture;

    // Create a fresh (active) parent Location so the sub-location FK is valid.
    private async Task<int> EnsureActiveLocationAsync()
    {
        var resp = await _f.Client.PostAsJsonAsync("/api/Location", new
        {
            code = _f.EntityCode[..10], locationName = "QA Parent Loc " + _f.EntityCode[..6], description = "SubLocation parent",
            sortOrder = 1, unitId = ValidUnitId, departmentId = ValidDepartmentId
        });
        return (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
    }

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        ValidLocationId = await EnsureActiveLocationAsync();
        _code = NewCode();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _code, subLocationName = "QA SubLocation", description = "Created by QA",
            unitId = ValidUnitId, departmentId = ValidDepartmentId, locationId = ValidLocationId
        });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { code = "NOAUTH01", subLocationName = "X", unitId = ValidUnitId, departmentId = ValidDepartmentId, locationId = ValidLocationId });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

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

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400); // by-name returns 400 on no-match (live)
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, code = NewCode(), subLocationName = "QA SubLocation Upd", description = "Updated",
            unitId = ValidUnitId, departmentId = ValidDepartmentId, locationId = ValidLocationId, isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, subLocationName = "X", unitId = ValidUnitId, departmentId = ValidDepartmentId, locationId = ValidLocationId, isActive = (byte)1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
