namespace FinanceManagement.QATests.Tests.MiscTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscTypeMaster (live-server QA) — Finance misc-type catalogue.
//
// Route: api/finance/misctypemaster
//   GET (list, paged+search) · GET {id} · GET by-name?term=
//   POST · PUT · DELETE ?id=   (MiscTypeCode immutable+alphanumeric, unique)
//
// Fully self-seeding (no FK dependency) → full CRUD lifecycle runs live.
// Create returns 200 with data = new id (int).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("FinMiscTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/misctypemaster";

    private static int _id;

    public MiscTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string suffix) => $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{suffix}";

    private object ValidCreateBody(string code) => new
    {
        miscTypeCode = code,
        description = $"QA Misc Type {_f.EntityCode}"
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, ValidCreateBody(Code("A")));
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
        var resp = await _f.Client.PostAsJsonAsync(Route, new { miscTypeCode = "", description = "" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadAsStringAsync()).ToLower().Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NonAlphanumericCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route,
            new { miscTypeCode = "QA-01", description = "Bad Code" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_DescriptionTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route,
            new { miscTypeCode = Code("ML"), description = new string('A', 251) });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Route, new { id = 0, description = "", isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL CRUD LIFECYCLE (self-seeding) ────────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_Create_HappyPath_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(Code("A")));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_Duplicate_Returns400()
    {
        if (_id == 0) return;
        // re-create with the same code → unique violation
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(Code("A")));
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
    public async Task TC013_ByName_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term={Code("A")}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Update_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = _id, description = $"QA Edited {_f.EntityCode}", isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Deactivate_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = _id, description = $"QA Edited {_f.EntityCode}", isActive = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_Delete_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.DeleteAsync($"{Route}?id={_id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
