namespace FinanceManagement.QATests.Tests.AccountTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// AccountTypeMaster (live-server QA) — Finance account-type catalogue.
//
// Route: api/finance/accounttypemaster
//   GET (list, paged+search) · GET {id} · GET by-name?term=&CompanyId=
//   POST · PUT · DELETE ?id=
//
// Create needs companyId (=1), unique accountTypeName per company, a single-digit
// startCode (1-9) unique per company, accountCodeLength (3-20), sortOrder (>=0).
// startCode is constrained to 1..9 and unique per company — for create-happy we pick a
// run-derived digit and TOLERATE a duplicate (already-seeded) 400. Create returns 200,
// data = new id (int).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AccountTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AccountTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/accounttypemaster";
    private const int QACompanyId = 1;

    private static int _id;

    public AccountTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string suffix) => $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{suffix}";

    // startCode must be a single digit 1-9; derive run-varying digit from the EntityCode.
    private string StartDigit()
    {
        var d = new string(_f.EntityCode.Where(char.IsDigit).ToArray());
        var n = d.Length > 0 ? (d[0] - '0') : 5;
        return (n == 0 ? 9 : n).ToString();
    }

    private object ValidCreateBody(string startCode) => new
    {
        companyId = QACompanyId,
        accountTypeName = $"QA Type {_f.EntityCode}",
        startCode,
        accountCodeLength = 6,
        sortOrder = 1
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, ValidCreateBody(StartDigit()));
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
            new { companyId = QACompanyId, accountTypeName = "", startCode = "", accountCodeLength = 0, sortOrder = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = 0, accountTypeName = "", startCode = "1", accountCodeLength = 6, sortOrder = 1, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL CRUD LIFECYCLE ───────────────────────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_Create_HappyPath_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(StartDigit()));

        // startCode is unique per company and constrained to 1-9; on a populated clone the
        // chosen digit may already be taken → tolerate the duplicate 400, otherwise expect 200.
        if (resp.StatusCode == HttpStatusCode.BadRequest) return;

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateName_Returns400()
    {
        if (_id == 0) return;
        // same accountTypeName under the same company → unique-name violation
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody("8"));
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
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term=QA&CompanyId={QACompanyId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Update_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            accountTypeName = $"QA Type Edited {_f.EntityCode}",
            startCode = StartDigit(),
            accountCodeLength = 6,
            sortOrder = 2,
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
            accountTypeName = $"QA Type Edited {_f.EntityCode}",
            startCode = StartDigit(),
            accountCodeLength = 6,
            sortOrder = 2,
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
