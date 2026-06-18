namespace FinanceManagement.QATests.Tests.TransactionTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// TransactionTypeMaster (live-server QA) — Finance transaction-type catalogue.
//
// Route: api/finance/transactiontypemaster
//   GET (list, paged+search) · GET {id} · GET by-name?term=
//   POST · PUT · DELETE ?id=
//
// FKs: moduleId → /api/Modules · menuId → /api/Menu (cross-module). Resolved at runtime
// via QAHelper.FirstIdAsync (fallback 1). typeName/shortName are unique per unit → run-unique
// via Code(). Create returns 200, data = new id (int).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("TransactionTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class TransactionTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/transactiontypemaster";

    private static int _id;
    private static int _moduleId;
    private static int _menuId;

    public TransactionTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string suffix) => $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{suffix}";

    private object ValidCreateBody(int moduleId, int menuId) => new
    {
        moduleId,
        menuId,
        typeName = Code("TN"),
        shortName = Code("SN"),
        description = $"QA Txn Type {_f.EntityCode}",
        isGate = 0
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, ValidCreateBody(1, 1));
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
        var resp = await _f.Client.PostAsJsonAsync(Route,
            new { moduleId = 0, menuId = 0, typeName = "", shortName = "", isGate = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = 0, typeName = "", shortName = "", isGate = 0, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL CRUD LIFECYCLE (FK-guarded) ──────────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_Create_HappyPath_CapturesId()
    {
        _moduleId = await QAHelper.FirstIdAsync(_f.Client, "/api/Modules");
        if (_moduleId == 0) _moduleId = 1;
        _menuId = await QAHelper.FirstIdAsync(_f.Client, "/api/Menu");
        if (_menuId == 0) _menuId = 1;

        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(_moduleId, _menuId));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateNames_Returns400()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(_moduleId, _menuId));
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
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term={Code("TN")}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Update_HappyPath_Returns200()
    {
        if (_id == 0) return;
        // UpdateTransactionTypeMasterCommand requires ModuleId + MenuId (validator: "ModuleId/MenuId
        // is required") — they are mutable and must be re-sent on every update, not just at create.
        var resp = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            moduleId = _moduleId,
            menuId = _menuId,
            typeName = Code("TN"),
            shortName = Code("SN"),
            description = $"QA Edited {_f.EntityCode}",
            isGate = 0,
            isActive = 1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Deactivate_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            moduleId = _moduleId,
            menuId = _menuId,
            typeName = Code("TN"),
            shortName = Code("SN"),
            description = $"QA Edited {_f.EntityCode}",
            isGate = 0,
            isActive = 0
        });
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
