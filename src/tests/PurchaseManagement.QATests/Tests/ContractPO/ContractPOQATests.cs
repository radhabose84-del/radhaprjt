namespace PurchaseManagement.QATests.Tests.ContractPO;

// ─────────────────────────────────────────────────────────────────────────────
// ContractPO — live-server QA suite (TRANSACTIONAL; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — ContractPOController):
//   POST   /api/ContractPO                          { ContractPOCreateDto }
//   PUT    /api/ContractPO                           { ContractPOUpdateDto }
//   DELETE /api/ContractPO/{id:int}
//   GET    /api/ContractPO/{id:int}                  (returns 200 + StatusCode:404 in body when not found)
//   POST   /api/ContractPO/amendment                 { ContractPOUpdateDto }
//   PUT    /api/ContractPO/cancel/{id:int}
//   PUT    /api/ContractPO/foreclose/{id:int}
//   GET    /api/ContractPO/pending?poId=&pageNumber=&pageSize=&searchTerm=
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid Contract PO requires a seeded ContractPOMaster header and a PO category — neither
//   guaranteed on the QA clone. These are attribute-level [Fact(Skip=...)] so they are explicit
//   pending work. The pending list (smoke), no-auth, empty-body POST, and GetById-nonexistent stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ContractPOCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ContractPOQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ContractPO";

    public ContractPOQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a ContractPOMaster header + PO category"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contractPOMasterId = 1,
            poCategoryId = 1,
            details = new[] { new { itemId = 1, quantity = 10m, rate = 100m } }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { contractPOMasterId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — PENDING LIST  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPending_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetPending_Page2_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=2&pageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (reachability; tolerant)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE / DELETE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created Contract PO id (TC001 is blocked on ContractPOMaster + PO category)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, contractPOMasterId = 1 });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a created Contract PO id (TC001 is blocked on ContractPOMaster + PO category)"), TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
