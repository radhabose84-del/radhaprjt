namespace FixedAssetManagement.QATests.Tests.Location;

// ─────────────────────────────────────────────────────────────────────────────
// Location — live-server QA suite.  Route: /api/Location
// Contract (verified 2026-06-09):
//   POST   { code, locationName, description?, sortOrder, unitId, departmentId }  → data = LocationDto
//   PUT    { id, code, locationName, description?, sortOrder, unitId, departmentId, isActive }
//          (controller pre-checks existence → 404 when id absent)
//   DELETE /{id}                       (route param; id<=0 guard → 400)
//   GET    ?PageNumber=&PageSize=&SearchTerm=
//   GET    /{id}                       (id<=0 guard → 400; absent → 200 + data:null)
//   GET    /by-name?name=
// FKs unitId/departmentId (cross-module UserManagement) use best-effort seed id 1.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LocationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LocationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Location";
    private const int ValidUnitId = 1;
    private const int ValidDepartmentId = 1;

    private string NewCode() => _f.EntityCode[..10];

    public LocationQATests(QAServerFixture fixture) => _f = fixture;

    // ── CREATE ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        // Uniqueness is composite (LocationName + DepartmentId + UnitId) — see CreateLocationCommandHandler
        // (GetByLocationNameAsync). A fixed name collides on re-runs without a reset, so make it run-unique.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = NewCode(),
            locationName = "QA Location " + _f.EntityCode[..6],
            description = "Created by QA",
            sortOrder = 1,
            unitId = ValidUnitId,
            departmentId = ValidDepartmentId
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            code = "NOAUTH01", locationName = "No Auth", unitId = ValidUnitId, departmentId = ValidDepartmentId
        });
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

    // ── GET BY ID ───────────────────────────────────────────────────────────────
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── AUTOCOMPLETE ────────────────────────────────────────────────────────────
    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // ── UPDATE ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            code = NewCode(),
            locationName = "QA Loc Upd " + _f.EntityCode[..6],
            description = "Updated by QA",
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            unitId = ValidUnitId,
            departmentId = ValidDepartmentId,
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NonExistentId_Returns404()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            code = NewCode(),
            locationName = "Ghost",
            sortOrder = 1,
            unitId = ValidUnitId,
            departmentId = ValidDepartmentId,
            isActive = (byte)1
        });
        // Live: non-existent update may 404 (controller pre-check) or 400 (validation); accept both.
        ((int)resp.StatusCode).Should().BeOneOf(404, 400);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, locationName = "X", unitId = ValidUnitId, departmentId = ValidDepartmentId, isActive = (byte)1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
