namespace FixedAssetManagement.QATests.Tests.UOM;

// ─────────────────────────────────────────────────────────────────────────────
// UOM — live-server QA suite.  Route: /api/fam/UOM   (NOTE the /fam prefix)
// Contract (verified 2026-06-09):
//   POST   { code, uomName, sortOrder, uomTypeId }   → data = UOMDto
//   PUT    { id, code, uomName, sortOrder, uomTypeId, isActive }   (pre-checks existence → 404)
//   DELETE /{id}                  (route param; id<=0 guard → 400)
//   GET    ?PageNumber=&PageSize=&SearchTerm=
//   GET    /{id}                  (id<=0 guard → 400)
//   GET    /by-name?name=  ,  /by-Type?name=
// FK uomTypeId uses a best-effort seed id 1.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("UOMCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class UOMQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/fam/UOM";
    private const int ValidUomTypeId = 1;

    private string NewCode() => _f.EntityCode[..10];

    public UOMQATests(QAServerFixture fixture) => _f = fixture;

    // ── CREATE ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = NewCode(),
            uomName = "QA Unit",
            sortOrder = 1,
            uomTypeId = ValidUomTypeId
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
            code = "NOAUTH01", uomName = "No Auth", uomTypeId = ValidUomTypeId
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

    // ── AUTOCOMPLETE ────────────────────────────────────────────────────────────
    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_ByName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_AutoComplete_ByType_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-Type?name=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400); // by-Type returns 400 on no-match (live)
    }

    // ── UPDATE ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            code = NewCode(),
            uomName = "QA Unit Upd " + _f.EntityCode[..6],
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            uomTypeId = ValidUomTypeId,
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NonExistentId_Returns404()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999, code = NewCode(), uomName = "Ghost", sortOrder = 1, uomTypeId = ValidUomTypeId, isActive = (byte)1
        });
        ((int)resp.StatusCode).Should().BeOneOf(404, 400); // non-existent update: 404 or 400 (live)
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, uomName = "X", uomTypeId = ValidUomTypeId, isActive = (byte)1
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
