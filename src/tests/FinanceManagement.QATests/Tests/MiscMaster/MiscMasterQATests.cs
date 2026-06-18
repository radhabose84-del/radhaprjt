namespace FinanceManagement.QATests.Tests.MiscMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscMaster (live-server QA) — Finance misc-value catalogue under a MiscType.
//
// Route: api/finance/miscmaster
//   GET (list, paged+search, &MiscTypeId=) · GET {id} · GET by-name?term=&MiscTypeCode=
//   POST · PUT · DELETE ?id=   (Code immutable+alphanumeric, unique per MiscType)
//
// FK: miscTypeId → /api/finance/misctypemaster (same-module). Resolved at runtime via
// QAHelper.FirstIdAsync; create-happy self-skips when no MiscType exists. Create returns
// 200 with data = new id (int).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("FinMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/miscmaster";
    private const string TypeRoute = "/api/finance/misctypemaster";

    private static int _id;
    private static int _miscTypeId;

    public MiscMasterQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string suffix) => $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{suffix}";

    private object ValidCreateBody(string code, int miscTypeId) => new
    {
        miscTypeId,
        code,
        description = $"QA Misc {_f.EntityCode}"
    };

    // ── SMOKE ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, ValidCreateBody(Code("A"), 1));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MissingRequired_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new { miscTypeId = 0, code = "", description = "" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_InvalidMiscTypeId_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(Code("FK"), 2147483647));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Route, new { id = 0, description = "", sortOrder = 1, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL CRUD LIFECYCLE (FK-guarded) ──────────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_Create_HappyPath_CapturesId()
    {
        _miscTypeId = await QAHelper.FirstIdAsync(_f.Client, TypeRoute);
        if (_miscTypeId == 0) return;   // no MiscType to parent under

        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(Code("A"), _miscTypeId));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateCodeWithinType_Returns400()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(Code("A"), _miscTypeId));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetById_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.GetAsync($"{Route}/{_id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_GetAll_FilteredByMiscTypeId_Reachable()
    {
        if (_miscTypeId == 0) return;
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15&MiscTypeId={_miscTypeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_ByName_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term={Code("A")}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Update_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = _id, description = $"QA Edited {_f.EntityCode}", sortOrder = 1, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_Deactivate_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = _id, description = $"QA Edited {_f.EntityCode}", sortOrder = 1, isActive = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_Delete_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.DeleteAsync($"{Route}?id={_id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
