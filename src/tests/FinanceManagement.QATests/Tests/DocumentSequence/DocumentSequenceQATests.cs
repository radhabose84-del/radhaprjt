namespace FinanceManagement.QATests.Tests.DocumentSequence;

// ─────────────────────────────────────────────────────────────────────────────
// DocumentSequence (live-server QA) — Finance document numbering sequences.
//
// Route: api/finance/documentsequence
//   GET (list, paged+search) · GET {id} · POST · PUT · DELETE ?id=
//
// FKs: transactionTypeId → /api/finance/transactiontypemaster · financialYearId →
// /api/FinancialYear. Composite unique (transactionType+financialYear+docNo). Resolved
// at runtime via QAHelper.FirstIdAsync; create-happy self-skips if either FK is 0. docNo
// is run-unique (QAHelper.RunUniqueInt). Create returns 200, data = new id (int).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DocumentSequenceCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DocumentSequenceQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/documentsequence";

    private static int _id;
    private static int _transactionTypeId;
    private static int _financialYearId;
    private static int _docNo;

    public DocumentSequenceQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private object ValidCreateBody(int transactionTypeId, int financialYearId, int docNo) => new
    {
        transactionTypeId,
        financialYearId,
        docNo
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, ValidCreateBody(1, 1, 1));
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
            new { transactionTypeId = 0, financialYearId = 0, docNo = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Route,
            new { id = 0, transactionTypeId = 1, financialYearId = 1, docNo = 1, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL CRUD LIFECYCLE (FK-guarded) ──────────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_Create_HappyPath_CapturesId()
    {
        _transactionTypeId = await QAHelper.FirstIdAsync(_f.Client, "/api/finance/transactiontypemaster");
        _financialYearId = await QAHelper.FirstIdAsync(_f.Client, "/api/FinancialYear");
        if (_transactionTypeId == 0 || _financialYearId == 0) return;   // no FK parents

        _docNo = QAHelper.RunUniqueInt(_f.EntityCode);
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(_transactionTypeId, _financialYearId, _docNo));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateComposite_Returns400()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PostAsJsonAsync(Route, ValidCreateBody(_transactionTypeId, _financialYearId, _docNo));
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
    public async Task TC013_Update_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            transactionTypeId = _transactionTypeId,
            financialYearId = _financialYearId,
            docNo = _docNo,
            isActive = 1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Deactivate_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            transactionTypeId = _transactionTypeId,
            financialYearId = _financialYearId,
            docNo = _docNo,
            isActive = 0
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Delete_HappyPath_Returns200()
    {
        if (_id == 0) return;
        var resp = await _f.Client.DeleteAsync($"{Route}?id={_id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
